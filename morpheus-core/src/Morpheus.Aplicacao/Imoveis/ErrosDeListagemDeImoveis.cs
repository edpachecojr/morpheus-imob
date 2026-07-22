using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Catálogo dos erros da listagem/busca de imóveis. Separado de
/// <see cref="Morpheus.Dominio.Imoveis.ErrosDeImovel"/> porque não são invariante de
/// domínio — são regra de consulta (paginação, filtro), própria da aplicação.
/// </summary>
public static class ErrosDeListagemDeImoveis
{
    public static Erro PaginaInvalida(int pagina) => new(
        "Imoveis.PaginaInvalida",
        $"Página deve ser 1 ou maior; recebido {pagina}.");

    public static Erro TamanhoDaPaginaInvalido(int tamanhoDaPagina) => new(
        "Imoveis.TamanhoDaPaginaInvalido",
        $"Tamanho da página deve estar entre 1 e {FiltroDeListagemDeImoveis.TamanhoMaximoDaPagina}; " +
        $"recebido {tamanhoDaPagina}.");

    public static Erro FinalidadeInvalida(string valorRecebido) => new(
        "Imoveis.FinalidadeInvalida",
        $"Finalidade inválida: '{valorRecebido}'. Valores aceitos: {string.Join(", ", Enum.GetNames<FinalidadeDoImovel>())}.");

    public static Erro SituacaoInvalida(string valorRecebido) => new(
        "Imoveis.SituacaoInvalida",
        $"Situação inválida: '{valorRecebido}'. Valores aceitos: {string.Join(", ", Enum.GetNames<SituacaoDoImovel>())}.");
}
