namespace Morpheus.Infraestrutura.Email;

/// <summary>
/// Configuração do provedor de e-mail transacional, lida da seção
/// <c>EmailTransacional</c> (variáveis <c>EmailTransacional__*</c>). Qualquer
/// SMTP compatível serve — provedor próprio, SES, SendGrid ou Postmark pelo
/// relay SMTP deles — sem prender o projeto a um SDK de fornecedor.
/// </summary>
public sealed class ConfiguracaoDeEmailTransacional
{
    public const string Secao = "EmailTransacional";

    public string Host { get; set; } = string.Empty;
    public int Porta { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Remetente { get; set; } = string.Empty;
    public string NomeDoRemetente { get; set; } = "Morpheus";

    /// <summary>
    /// Base do link enviado ao usuário (redefinição de senha, confirmação de
    /// e-mail). Sem barra final. Ex.: <c>https://painel.morpheus.exemplo</c>.
    /// </summary>
    public string UrlDoPainel { get; set; } = string.Empty;
}
