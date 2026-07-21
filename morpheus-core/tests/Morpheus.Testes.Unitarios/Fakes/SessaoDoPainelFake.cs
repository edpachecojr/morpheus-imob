using Morpheus.Aplicacao.Sessoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>Registra quais sessões foram abertas e quais revogações aconteceram.</summary>
public sealed class SessaoDoPainelFake : ISessaoDoPainel
{
    public List<Guid> Abertas { get; } = [];
    public List<Guid> RevogadasPorUsuario { get; } = [];
    public int Encerramentos { get; private set; }

    public Task AbrirAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        Abertas.Add(usuarioId);
        return Task.CompletedTask;
    }

    public Task EncerrarAsync(CancellationToken cancelamento)
    {
        Encerramentos++;
        return Task.CompletedTask;
    }

    public Task EncerrarTodasDoUsuarioAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        RevogadasPorUsuario.Add(usuarioId);
        return Task.CompletedTask;
    }
}
