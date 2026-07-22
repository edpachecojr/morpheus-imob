using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Critério de busca e paginação da listagem de imóveis (E2-F1-H2). <see cref="Busca"/>
/// casa contra código de referência, título ou endereço; <see cref="Finalidade"/> e
/// <see cref="Situacao"/> são exatos. A organização não é campo daqui — vem do
/// contexto de quem consulta, nunca do cliente.
/// Exemplo: <c>new FiltroDeListagemDeImoveis(Busca: "acácias", Finalidade: null, Situacao: null, Pagina: 1, TamanhoDaPagina: 20)</c>.
/// </summary>
public sealed record FiltroDeListagemDeImoveis(
    string? Busca,
    FinalidadeDoImovel? Finalidade,
    SituacaoDoImovel? Situacao,
    int Pagina = 1,
    int TamanhoDaPagina = 20)
{
    public const int TamanhoPadraoDaPagina = 20;
    public const int TamanhoMaximoDaPagina = 100;

    /// <summary>
    /// Valida os limites de paginação. Busca e filtros são livres — string vazia ou
    /// enum ausente apenas não restringem o resultado.
    /// Exemplo: <c>filtro.Validar()</c>.
    /// </summary>
    public Resultado Validar()
    {
        if (Pagina < 1)
            return Resultado.DeFalha(ErrosDeListagemDeImoveis.PaginaInvalida(Pagina));
        if (TamanhoDaPagina is < 1 or > TamanhoMaximoDaPagina)
            return Resultado.DeFalha(ErrosDeListagemDeImoveis.TamanhoDaPaginaInvalido(TamanhoDaPagina));
        return Resultado.DeSucesso();
    }
}
