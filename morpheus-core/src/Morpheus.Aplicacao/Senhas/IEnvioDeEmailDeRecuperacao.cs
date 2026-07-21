namespace Morpheus.Aplicacao.Senhas;

/// <summary>
/// Entrega ao usuário o link de redefinição de senha. Porta separada do provedor
/// de tokens porque muda por outra razão: trocar de serviço de e-mail não mexe em
/// como o token é emitido.
/// <para>
/// O token é parâmetro e não campo persistido: ele não passa pelo outbox nem pelo
/// log — credencial portadora não descansa em tabela nossa.
/// </para>
/// </summary>
public interface IEnvioDeEmailDeRecuperacao
{
    Task EnviarAsync(string email, string nomeCompleto, string token, CancellationToken cancelamento);
}
