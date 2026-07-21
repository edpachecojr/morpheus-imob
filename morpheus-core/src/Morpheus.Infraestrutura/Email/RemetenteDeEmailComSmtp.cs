using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Morpheus.Infraestrutura.Email;

/// <summary>
/// Envolve o MailKit atrás de um método só — o domínio e a aplicação nunca
/// veem <c>SmtpClient</c> nem <c>MimeMessage</c> (CLAUDE.md §Dependências).
/// Compartilhado entre os envios de recuperação de senha e de confirmação de
/// e-mail, que só diferem no assunto e no corpo da mensagem.
/// </summary>
public sealed class RemetenteDeEmailComSmtp
{
    private readonly ConfiguracaoDeEmailTransacional _configuracao;

    public RemetenteDeEmailComSmtp(IOptions<ConfiguracaoDeEmailTransacional> configuracao)
        => _configuracao = configuracao.Value;

    public async Task EnviarAsync(
        string destinatario, string nomeDoDestinatario, string assunto, string corpo, CancellationToken cancelamento)
    {
        ValidarConfiguracao();

        using var mensagem = MontarMensagem(destinatario, nomeDoDestinatario, assunto, corpo);
        using var cliente = new SmtpClient();
        await cliente.ConnectAsync(
            _configuracao.Host, _configuracao.Porta, SecureSocketOptions.StartTlsWhenAvailable, cancelamento);
        await cliente.AuthenticateAsync(_configuracao.Usuario, _configuracao.Senha, cancelamento);
        await cliente.SendAsync(mensagem, cancelamento);
        await cliente.DisconnectAsync(true, cancelamento);
    }

    private MimeMessage MontarMensagem(
        string destinatario, string nomeDoDestinatario, string assunto, string corpo)
    {
        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_configuracao.NomeDoRemetente, _configuracao.Remetente));
        mensagem.To.Add(new MailboxAddress(nomeDoDestinatario, destinatario));
        mensagem.Subject = assunto;
        mensagem.Body = new TextPart("plain") { Text = corpo };
        return mensagem;
    }

    // Falha no primeiro envio, não na subida: e-mail não é do caminho crítico de
    // toda requisição, diferente da conexão com o banco (VariaveisDeAmbienteObrigatorias).
    private void ValidarConfiguracao()
    {
        if (!string.IsNullOrWhiteSpace(_configuracao.Host) && !string.IsNullOrWhiteSpace(_configuracao.Remetente))
            return;

        throw new InvalidOperationException(
            "Configuração obrigatória ausente: 'EmailTransacional__Host' e 'EmailTransacional__Remetente'. " +
            "Formato esperado: host SMTP (ex. 'smtp.seuprovedor.com') e um e-mail remetente válido.");
    }
}
