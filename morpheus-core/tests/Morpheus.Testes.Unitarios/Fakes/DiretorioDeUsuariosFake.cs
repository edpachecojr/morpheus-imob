using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>Diretório de usuários em memória, indexado por e-mail.</summary>
public sealed class DiretorioDeUsuariosFake : IDiretorioDeUsuarios
{
    private readonly Dictionary<string, UsuarioDoPainel> _porEmail = new(StringComparer.OrdinalIgnoreCase);

    public UsuarioDoPainel Registrar(string email, bool bloqueado = false)
    {
        var usuario = new UsuarioDoPainel(Guid.NewGuid(), email, "Ana Souza", bloqueado);
        _porEmail[email] = usuario;
        return usuario;
    }

    public Task<UsuarioDoPainel?> BuscarPorEmailAsync(string email, CancellationToken cancelamento)
        => Task.FromResult(_porEmail.GetValueOrDefault(email));

    public Task<IReadOnlyList<UsuarioDoPainel>> ListarDaOrganizacaoAsync(CancellationToken cancelamento)
        => Task.FromResult<IReadOnlyList<UsuarioDoPainel>>([.. _porEmail.Values]);
}
