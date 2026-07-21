using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Morpheus.Aplicacao.Sessoes;

namespace Morpheus.Api.Identidade;

/// <summary>
/// Ponte entre o cookie de autenticação e o armazenamento de sessões: com um
/// <see cref="ITicketStore"/> configurado, o cookie deixa de carregar a identidade
/// cifrada e passa a carregar só a chave da sessão — que é o que torna a revogação
/// imediata possível (ADR-0011).
/// </summary>
public sealed class TicketStoreDeSessoes : ITicketStore
{
    /// <summary>Validade usada quando o ticket não traz expiração própria.</summary>
    private static readonly TimeSpan ValidadePadrao = TimeSpan.FromDays(14);

    private readonly IArmazenamentoDeSessoes _sessoes;
    private readonly TimeProvider _relogio;

    public TicketStoreDeSessoes(IArmazenamentoDeSessoes sessoes, TimeProvider relogio)
    {
        _sessoes = sessoes;
        _relogio = relogio;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var sessao = Converter(ticket, Guid.NewGuid());
        await _sessoes.GuardarAsync(sessao, CancellationToken.None);
        return sessao.Id.ToString();
    }

    public async Task RenewAsync(string chave, AuthenticationTicket ticket)
    {
        if (Guid.TryParse(chave, out var sessaoId))
            await _sessoes.RenovarAsync(Converter(ticket, sessaoId), CancellationToken.None);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string chave)
    {
        if (!Guid.TryParse(chave, out var sessaoId))
            return null;

        var sessao = await _sessoes.BuscarAsync(sessaoId, CancellationToken.None);
        return sessao is null ? null : TicketSerializer.Default.Deserialize(sessao.Conteudo);
    }

    public async Task RemoveAsync(string chave)
    {
        if (Guid.TryParse(chave, out var sessaoId))
            await _sessoes.RemoverAsync(sessaoId, CancellationToken.None);
    }

    private SessaoPersistida Converter(AuthenticationTicket ticket, Guid sessaoId) => new(
        sessaoId,
        LerUsuario(ticket),
        TicketSerializer.Default.Serialize(ticket),
        ticket.Properties.ExpiresUtc ?? _relogio.GetUtcNow().Add(ValidadePadrao));

    // Guardar o dono junto do ticket é o que permite revogar todas as sessões de
    // um usuário sem abrir e desserializar cada linha da tabela.
    private static Guid LerUsuario(AuthenticationTicket ticket)
    {
        var identificador = ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(identificador, out var usuarioId))
            throw new InvalidOperationException(
                $"Sessão sem identificador de usuário utilizável: recebido '{identificador ?? "<ausente>"}', " +
                "esperado um GUID na claim NameIdentifier.");
        return usuarioId;
    }
}
