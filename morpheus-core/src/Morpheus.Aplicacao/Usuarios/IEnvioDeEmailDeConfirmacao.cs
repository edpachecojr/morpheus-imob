namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Entrega ao usuário o link de confirmação de e-mail (E1-F2-H6). Porta
/// separada do provedor de tokens pela mesma razão da recuperação de senha:
/// trocar de serviço de e-mail não mexe em como o token é emitido.
/// </summary>
public interface IEnvioDeEmailDeConfirmacao
{
    /// <summary>
    /// Envia o e-mail com o link de confirmação.
    /// Exemplo: <c>await envio.EnviarAsync(email, nomeCompleto, token, cancelamento)</c>.
    /// </summary>
    Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento);
}
