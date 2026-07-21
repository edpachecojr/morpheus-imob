using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Verificador de senha que conta cada operação. O contador de
/// <see cref="DerivacoesDeChave"/> é o que prova a equalização de tempo do login:
/// todo modo de recusa precisa ter derivado uma chave, exista conta ou não.
/// </summary>
public sealed class VerificadorDeSenhaFake : IVerificadorDeSenha
{
    private readonly Dictionary<Guid, string> _senhas = [];

    public int DerivacoesDeChave { get; private set; }
    public int FalhasRegistradas { get; private set; }
    public int LimpezasDeFalhas { get; private set; }

    public void DefinirSenha(Guid usuarioId, string senha) => _senhas[usuarioId] = senha;

    public Task<bool> ConferirAsync(Guid usuarioId, string senha, CancellationToken cancelamento)
    {
        DerivacoesDeChave++;
        return Task.FromResult(_senhas.TryGetValue(usuarioId, out var esperada) && esperada == senha);
    }

    public Task ConsumirTempoEquivalenteAsync(CancellationToken cancelamento)
    {
        DerivacoesDeChave++;
        return Task.CompletedTask;
    }

    public Task RegistrarFalhaAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        FalhasRegistradas++;
        return Task.CompletedTask;
    }

    public Task LimparFalhasAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        LimpezasDeFalhas++;
        return Task.CompletedTask;
    }
}
