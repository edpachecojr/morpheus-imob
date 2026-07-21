using Morpheus.Aplicacao.Sessoes;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Senhas;

/// <summary>
/// Troca a senha a partir do token de recuperação (E1-F2-H3) e derruba todas as
/// sessões abertas do usuário. Derrubar é o ponto: quem redefine a senha
/// tipicamente suspeita de acesso indevido, e deixar a sessão do invasor viva
/// tornaria a redefinição inútil.
/// </summary>
public sealed class RedefinicaoDeSenha
{
    private readonly IDiretorioDeUsuarios _usuarios;
    private readonly ITokensDeRecuperacaoDeSenha _tokens;
    private readonly ISessaoDoPainel _sessao;

    public RedefinicaoDeSenha(
        IDiretorioDeUsuarios usuarios, ITokensDeRecuperacaoDeSenha tokens, ISessaoDoPainel sessao)
    {
        _usuarios = usuarios;
        _tokens = tokens;
        _sessao = sessao;
    }

    /// <summary>
    /// Redefine a senha e revoga as sessões. E-mail sem conta devolve o mesmo erro
    /// de token inválido — não confirma se a conta existe.
    /// Exemplo: <c>await redefinicao.ExecutarAsync(email, token, novaSenha, cancelamento)</c>.
    /// </summary>
    public async Task<Resultado> ExecutarAsync(
        string email, string token, string novaSenha, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.BuscarPorEmailAsync(email, cancelamento);
        if (usuario is null)
            return Resultado.DeFalha(ErrosDeAutenticacao.TokenDeRecuperacaoInvalido);

        var redefinicao = await _tokens.RedefinirAsync(usuario.Id, token, novaSenha, cancelamento);
        if (redefinicao.Falha)
            return redefinicao;

        await _sessao.EncerrarTodasDoUsuarioAsync(usuario.Id, cancelamento);
        return Resultado.DeSucesso();
    }
}
