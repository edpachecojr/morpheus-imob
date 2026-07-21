using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Morpheus.Aplicacao.Senhas;

namespace Morpheus.Infraestrutura.Email;

/// <summary>
/// Envia o link de redefinição de senha (E1-F2-H3) por e-mail transacional de
/// verdade, substituindo <c>EnvioDeRecuperacaoRegistradoEmLog</c>. O token
/// <b>nunca</b> é logado: log com token é credencial portadora em texto plano
/// num sistema de busca (CLAUDE.md §Logging).
/// </summary>
public sealed class EnvioDeRecuperacaoPorEmail : IEnvioDeEmailDeRecuperacao
{
    private readonly RemetenteDeEmailComSmtp _remetente;
    private readonly ConfiguracaoDeEmailTransacional _configuracao;
    private readonly ILogger<EnvioDeRecuperacaoPorEmail> _diario;

    public EnvioDeRecuperacaoPorEmail(
        RemetenteDeEmailComSmtp remetente,
        IOptions<ConfiguracaoDeEmailTransacional> configuracao,
        ILogger<EnvioDeRecuperacaoPorEmail> diario)
    {
        _remetente = remetente;
        _configuracao = configuracao.Value;
        _diario = diario;
    }

    public async Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento)
    {
        var link = $"{_configuracao.UrlDoPainel}/redefinir-senha" +
            $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        await _remetente.EnviarAsync(
            email, nomeCompleto, "Redefinição de senha — Morpheus", CorpoDoEmail(nomeCompleto, link), cancelamento);

        _diario.LogInformation(
            "Link de redefinição de senha enviado para {destinatario_mascarado}",
            MascaramentoDeEmail.Mascarar(email));
    }

    private static string CorpoDoEmail(string nomeCompleto, string link) =>
        $"Olá, {nomeCompleto}.\n\n" +
        "Use o link abaixo para redefinir sua senha. Ele expira em 30 minutos e só pode ser usado uma vez.\n\n" +
        $"{link}\n\n" +
        "Se você não pediu isso, ignore este e-mail.";
}
