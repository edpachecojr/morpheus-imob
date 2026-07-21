using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Infraestrutura.Email;

/// <summary>
/// Envia o link de confirmação de e-mail do cadastro (E1-F2-H6). O token
/// <b>nunca</b> é logado, só o desfecho — mesma regra do envio de recuperação
/// de senha (CLAUDE.md §Logging).
/// </summary>
public sealed class EnvioDeConfirmacaoPorEmail : IEnvioDeEmailDeConfirmacao
{
    private readonly RemetenteDeEmailComSmtp _remetente;
    private readonly ConfiguracaoDeEmailTransacional _configuracao;
    private readonly ILogger<EnvioDeConfirmacaoPorEmail> _diario;

    public EnvioDeConfirmacaoPorEmail(
        RemetenteDeEmailComSmtp remetente,
        IOptions<ConfiguracaoDeEmailTransacional> configuracao,
        ILogger<EnvioDeConfirmacaoPorEmail> diario)
    {
        _remetente = remetente;
        _configuracao = configuracao.Value;
        _diario = diario;
    }

    public async Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento)
    {
        var link = $"{_configuracao.UrlDoPainel}/confirmar-email" +
            $"?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        await _remetente.EnviarAsync(
            email, nomeCompleto, "Confirme seu e-mail — Morpheus", CorpoDoEmail(nomeCompleto, link), cancelamento);

        _diario.LogInformation(
            "Link de confirmação de e-mail enviado para {destinatario_mascarado}",
            MascaramentoDeEmail.Mascarar(email));
    }

    private static string CorpoDoEmail(string nomeCompleto, string link) =>
        $"Olá, {nomeCompleto}.\n\n" +
        "Confirme seu e-mail para garantir que a recuperação de senha e os avisos de cobrança cheguem até você.\n\n" +
        $"{link}\n\n" +
        "Sua conta já funciona normalmente mesmo sem confirmar — isso é só uma garantia a mais.";
}
