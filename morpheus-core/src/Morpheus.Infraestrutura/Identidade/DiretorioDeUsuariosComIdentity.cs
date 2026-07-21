using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Infraestrutura.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Consulta usuários do painel. A busca por e-mail atravessa organizações de
/// propósito — no login ainda não há tenant no contexto, ele nasce justamente do
/// usuário encontrado. Já a listagem passa pelo filtro explícito de organização,
/// como qualquer leitura de dado de negócio.
/// </summary>
public sealed class DiretorioDeUsuariosComIdentity : IDiretorioDeUsuarios
{
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;
    private readonly MorpheusDbContext _banco;
    private readonly IContextoDaOrganizacaoAtual _organizacao;
    private readonly TimeProvider _relogio;

    public DiretorioDeUsuariosComIdentity(
        UserManager<UsuarioDaOrganizacao> usuarios,
        MorpheusDbContext banco,
        IContextoDaOrganizacaoAtual organizacao,
        TimeProvider relogio)
    {
        _usuarios = usuarios;
        _banco = banco;
        _organizacao = organizacao;
        _relogio = relogio;
    }

    public async Task<UsuarioDoPainel?> BuscarPorEmailAsync(string email, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByEmailAsync(email);
        if (usuario is null)
            return null;

        var bloqueado = await _usuarios.IsLockedOutAsync(usuario);
        return Projetar(usuario, bloqueado);
    }

    public async Task<UsuarioDoPainel?> BuscarPorIdAsync(Guid id, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByIdAsync(id.ToString());
        if (usuario is null)
            return null;

        var bloqueado = await _usuarios.IsLockedOutAsync(usuario);
        return Projetar(usuario, bloqueado);
    }

    public async Task<IReadOnlyList<UsuarioDoPainel>> ListarDaOrganizacaoAsync(CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacao.ObterOrganizacaoIdAsync(cancelamento);
        var usuarios = await _banco.Users
            .DaOrganizacao(organizacaoId)
            .AsNoTracking()
            .OrderBy(usuario => usuario.NomeCompleto)
            .ToListAsync(cancelamento);

        var agora = _relogio.GetUtcNow();
        return [.. usuarios.Select(usuario => Projetar(usuario, EstaBloqueadoEm(usuario, agora)))];
    }

    private static UsuarioDoPainel Projetar(UsuarioDaOrganizacao usuario, bool bloqueado)
        => new(usuario.Id, usuario.Email ?? string.Empty, usuario.NomeCompleto, bloqueado);

    // Espelha a regra do UserManager sem uma ida ao banco por usuário da lista.
    private static bool EstaBloqueadoEm(UsuarioDaOrganizacao usuario, DateTimeOffset agora)
        => usuario.LockoutEnabled && usuario.LockoutEnd > agora;
}
