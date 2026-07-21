using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Emite e consome o token de confirmação de e-mail (E1-F2-H6). Porta separada
/// de <see cref="Senhas.ITokensDeRecuperacaoDeSenha"/> porque, embora os dois
/// apoiem no mesmo provedor de tokens do Identity, representam propósitos
/// diferentes — confirmar posse de e-mail não é recuperar acesso.
/// </summary>
public interface ITokensDeConfirmacaoDeEmail
{
    /// <summary>
    /// Emite o primeiro token de confirmação, logo após o cadastro.
    /// Exemplo: <c>await tokens.GerarAsync(usuarioId, cancelamento)</c>.
    /// </summary>
    Task<string> GerarAsync(Guid usuarioId, CancellationToken cancelamento);

    /// <summary>
    /// Emite um novo token para reenvio, invalidando qualquer token anterior
    /// ainda pendente — critério de aceite do reenvio (E1-F2-H6).
    /// Exemplo: <c>await tokens.RenovarAsync(usuarioId, cancelamento)</c>.
    /// </summary>
    Task<string> RenovarAsync(Guid usuarioId, CancellationToken cancelamento);

    /// <summary>
    /// Confirma o e-mail se o token for válido e ainda não tiver expirado.
    /// Exemplo: <c>await tokens.ConfirmarAsync(usuarioId, token, cancelamento)</c>.
    /// </summary>
    Task<Resultado> ConfirmarAsync(Guid usuarioId, string token, CancellationToken cancelamento);
}
