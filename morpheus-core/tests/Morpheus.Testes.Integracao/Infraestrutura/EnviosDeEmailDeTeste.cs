using Morpheus.Aplicacao.Senhas;
using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Substitui os dois envios de e-mail transacional (recuperação de senha e
/// confirmação de cadastro) nos testes de integração: guarda os envios em
/// memória em vez de abrir uma conexão SMTP real, mantendo a suíte sem rede
/// (CLAUDE.md §Testes — Repeatable).
/// </summary>
public sealed class EnviosDeEmailDeTeste : IEnvioDeEmailDeRecuperacao, IEnvioDeEmailDeConfirmacao
{
    public sealed record Envio(string Email, string NomeCompleto, string Token);

    public List<Envio> Envios { get; } = [];

    public Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento)
    {
        Envios.Add(new Envio(email, nomeCompleto, token));
        return Task.CompletedTask;
    }
}
