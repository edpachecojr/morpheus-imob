using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Consome o token do link de confirmação de e-mail (E1-F2-H6). Anônimo por
/// natureza: o link chega pela caixa de entrada, não por uma sessão ativa.
/// </summary>
public sealed class ConfirmacaoDeEmail
{
    private readonly IDiretorioDeUsuarios _usuarios;
    private readonly ITokensDeConfirmacaoDeEmail _tokens;

    public ConfirmacaoDeEmail(IDiretorioDeUsuarios usuarios, ITokensDeConfirmacaoDeEmail tokens)
    {
        _usuarios = usuarios;
        _tokens = tokens;
    }

    /// <summary>
    /// Confirma o e-mail a partir do link. E-mail sem conta devolve o mesmo erro
    /// de token inválido — não confirma se a conta existe.
    /// Exemplo: <c>await confirmacao.ExecutarAsync(email, token, cancelamento)</c>.
    /// </summary>
    public async Task<Resultado> ExecutarAsync(string email, string token, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.BuscarPorEmailAsync(email, cancelamento);
        if (usuario is null)
            return Resultado.DeFalha(ErrosDeAutenticacao.TokenDeConfirmacaoInvalido);

        return await _tokens.ConfirmarAsync(usuario.Id, token, cancelamento);
    }
}
