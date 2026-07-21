using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Aplicacao.Senhas;

/// <summary>
/// Pedido de redefinição de senha (E1-F2-H3). Não devolve resultado de propósito:
/// exista ou não a conta, quem chama responde a mesma coisa — e-mail inexistente
/// que respondesse diferente transformaria o formulário em verificador de contas.
/// </summary>
public sealed class SolicitacaoDeRecuperacaoDeSenha
{
    private readonly IDiretorioDeUsuarios _usuarios;
    private readonly ITokensDeRecuperacaoDeSenha _tokens;
    private readonly IEnvioDeEmailDeRecuperacao _envio;

    public SolicitacaoDeRecuperacaoDeSenha(
        IDiretorioDeUsuarios usuarios,
        ITokensDeRecuperacaoDeSenha tokens,
        IEnvioDeEmailDeRecuperacao envio)
    {
        _usuarios = usuarios;
        _tokens = tokens;
        _envio = envio;
    }

    /// <summary>
    /// Emite e envia o link quando a conta existe; não faz nada quando não existe.
    /// Exemplo: <c>await solicitacao.ExecutarAsync("ana@exemplo.com", cancelamento)</c>.
    /// </summary>
    public async Task ExecutarAsync(string email, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.BuscarPorEmailAsync(email, cancelamento);
        if (usuario is null)
            return;

        var token = await _tokens.GerarAsync(usuario.Id, cancelamento);
        await _envio.EnviarAsync(usuario.Email, usuario.NomeCompleto, token, cancelamento);
    }
}
