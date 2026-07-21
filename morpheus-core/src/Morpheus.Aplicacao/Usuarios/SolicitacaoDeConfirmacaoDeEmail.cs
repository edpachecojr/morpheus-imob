using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Reenvia a confirmação de e-mail para o usuário da sessão corrente (E1-F2-H6).
/// Exige sessão — diferente da recuperação de senha, aqui não há risco de
/// enumeração: quem pede o reenvio já provou identidade ao logar.
/// </summary>
public sealed class SolicitacaoDeConfirmacaoDeEmail
{
    private readonly IContextoDoUsuario _usuarioAtual;
    private readonly IDiretorioDeUsuarios _usuarios;
    private readonly ITokensDeConfirmacaoDeEmail _tokens;
    private readonly IEnvioDeEmailDeConfirmacao _envio;

    public SolicitacaoDeConfirmacaoDeEmail(
        IContextoDoUsuario usuarioAtual,
        IDiretorioDeUsuarios usuarios,
        ITokensDeConfirmacaoDeEmail tokens,
        IEnvioDeEmailDeConfirmacao envio)
    {
        _usuarioAtual = usuarioAtual;
        _usuarios = usuarios;
        _tokens = tokens;
        _envio = envio;
    }

    /// <summary>
    /// Emite um token novo — invalidando o anterior — e reenvia o e-mail.
    /// Exemplo: <c>await solicitacao.ExecutarAsync(cancelamento)</c>.
    /// </summary>
    public async Task ExecutarAsync(CancellationToken cancelamento)
    {
        var usuarioId = _usuarioAtual.UsuarioAutenticadoId ?? throw new ErroDeUsuarioNaoAutenticado();
        var usuario = await _usuarios.BuscarPorIdAsync(usuarioId, cancelamento)
            ?? throw new InvalidOperationException(
                $"Usuário {usuarioId} da sessão não existe mais no registro.");

        var token = await _tokens.RenovarAsync(usuarioId, cancelamento);
        await _envio.EnviarAsync(usuario.Email, usuario.NomeCompleto, token, cancelamento);
    }
}
