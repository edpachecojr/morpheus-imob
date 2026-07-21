using Microsoft.AspNetCore.Identity;
using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Confere senha e contabiliza falhas pelo <c>UserManager</c>, que já traz o
/// bloqueio por tentativas — o "rate limit por conta" de
/// [autenticacao.md](../../../../docs/fundacao/autenticacao.md).
/// </summary>
public sealed class VerificadorDeSenhaComIdentity : IVerificadorDeSenha
{
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;
    private readonly IPasswordHasher<UsuarioDaOrganizacao> _hasher;
    private readonly SenhaDeReferenciaParaEqualizarTempo _referencia;

    public VerificadorDeSenhaComIdentity(
        UserManager<UsuarioDaOrganizacao> usuarios,
        IPasswordHasher<UsuarioDaOrganizacao> hasher,
        SenhaDeReferenciaParaEqualizarTempo referencia)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _referencia = referencia;
    }

    public async Task<bool> ConferirAsync(Guid usuarioId, string senha, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByIdAsync(usuarioId.ToString());
        return usuario is not null && await _usuarios.CheckPasswordAsync(usuario, senha);
    }

    public Task ConsumirTempoEquivalenteAsync(CancellationToken cancelamento)
    {
        // O resultado é descartado: o que importa é ter derivado a chave.
        _hasher.VerifyHashedPassword(new UsuarioDaOrganizacao(), _referencia.Hash, "sem-conta");
        return Task.CompletedTask;
    }

    public async Task RegistrarFalhaAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByIdAsync(usuarioId.ToString());
        if (usuario is not null)
            await _usuarios.AccessFailedAsync(usuario);
    }

    public async Task LimparFalhasAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByIdAsync(usuarioId.ToString());
        if (usuario is not null)
            await _usuarios.ResetAccessFailedCountAsync(usuario);
    }
}
