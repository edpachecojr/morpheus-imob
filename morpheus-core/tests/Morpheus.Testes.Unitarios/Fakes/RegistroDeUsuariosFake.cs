using Morpheus.Aplicacao.Contas;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Registro de usuários em memória. Guarda os e-mails criados e conta as chamadas,
/// para o teste provar tanto o que foi criado quanto o que <b>não</b> chegou a ser
/// tentado (o cadastro de e-mail repetido não deve criar nada).
/// </summary>
public sealed class RegistroDeUsuariosFake : IRegistroDeUsuarios
{
    private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase);

    public List<NovoUsuarioDoPainel> Criados { get; } = [];
    public int TentativasDeCriacao { get; private set; }
    public Erro? SenhaRecusadaCom { get; set; }
    public Erro? CriacaoRecusadaCom { get; set; }

    public void JaCadastrado(string email) => _emails.Add(email);

    public Task<Resultado> ValidarSenhaAsync(string senha, CancellationToken cancelamento)
        => Task.FromResult(SenhaRecusadaCom is null
            ? Resultado.DeSucesso()
            : Resultado.DeFalha(SenhaRecusadaCom));

    public Task<bool> ExisteComEmailAsync(string email, CancellationToken cancelamento)
        => Task.FromResult(_emails.Contains(email));

    public Task<Resultado<Guid>> CriarAsync(NovoUsuarioDoPainel novo, CancellationToken cancelamento)
    {
        TentativasDeCriacao++;
        if (CriacaoRecusadaCom is not null)
            return Task.FromResult(Resultado.DeFalha<Guid>(CriacaoRecusadaCom));

        _emails.Add(novo.Email);
        Criados.Add(novo);
        return Task.FromResult(Resultado.DeSucesso(Guid.NewGuid()));
    }
}
