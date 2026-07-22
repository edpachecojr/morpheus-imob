namespace Morpheus.Aplicacao.Comum;

/// <summary>
/// Página de um resultado maior: os itens da página pedida mais o total de itens
/// que casam com o filtro (sem paginação), para o consumidor montar navegação sem
/// uma segunda chamada. Convenção única de paginação do sistema — todo caso de uso
/// que lista algo grande devolve isto, não uma lista crua.
/// Exemplo: <c>new ResultadoPaginado&lt;ImovelResumo&gt;(itens, total: 42, pagina: 1, tamanhoDaPagina: 20)</c>.
/// </summary>
public sealed record ResultadoPaginado<TItem>(
    IReadOnlyList<TItem> Itens, int Total, int Pagina, int TamanhoDaPagina)
{
    public int TotalDePaginas => TamanhoDaPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoDaPagina);
}
