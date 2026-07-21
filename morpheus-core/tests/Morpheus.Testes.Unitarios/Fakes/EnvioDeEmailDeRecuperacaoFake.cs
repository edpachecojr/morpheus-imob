using Morpheus.Aplicacao.Senhas;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>Guarda os envios de link de redefinição, para o teste conferir destino e token.</summary>
public sealed class EnvioDeEmailDeRecuperacaoFake : IEnvioDeEmailDeRecuperacao
{
    public sealed record Envio(string Email, string NomeCompleto, string Token);

    public List<Envio> Envios { get; } = [];

    public Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento)
    {
        Envios.Add(new Envio(email, nomeCompleto, token));
        return Task.CompletedTask;
    }
}
