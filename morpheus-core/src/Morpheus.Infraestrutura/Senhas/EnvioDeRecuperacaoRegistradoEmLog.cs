using Microsoft.Extensions.Logging;
using Morpheus.Aplicacao.Senhas;

namespace Morpheus.Infraestrutura.Senhas;

/// <summary>
/// Implementação de transição do envio do link de redefinição: registra que um
/// link foi emitido, sem o token e sem o e-mail em claro. Existe para que o fluxo
/// de recuperação (E1-F2-H3) esteja completo e testável antes de o provedor de
/// e-mail transacional entrar — trocar por ele é registrar outra implementação
/// desta porta, sem tocar no caso de uso.
/// <para>
/// O token <b>nunca</b> é logado: log com token é credencial portadora em texto
/// plano num sistema de busca (CLAUDE.md §Logging).
/// </para>
/// </summary>
public sealed class EnvioDeRecuperacaoRegistradoEmLog : IEnvioDeEmailDeRecuperacao
{
    private readonly ILogger<EnvioDeRecuperacaoRegistradoEmLog> _diario;

    public EnvioDeRecuperacaoRegistradoEmLog(ILogger<EnvioDeRecuperacaoRegistradoEmLog> diario)
        => _diario = diario;

    public Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento)
    {
        _diario.LogInformation(
            "Link de redefinição de senha emitido para {destinatario_mascarado}",
            Mascarar(email));
        return Task.CompletedTask;
    }

    // "ana.souza@exemplo.com" → "a***@exemplo.com": o suficiente para casar com um
    // chamado de suporte, insuficiente para reconstruir a lista de clientes.
    private static string Mascarar(string email)
    {
        var arroba = email.IndexOf('@', StringComparison.Ordinal);
        if (arroba <= 0)
            return "***";
        return $"{email[0]}***{email[arroba..]}";
    }
}
