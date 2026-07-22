using Morpheus.Aplicacao.Comum;

namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Leitura performática de imóveis via Dapper, com o filtro de organização
/// sempre explícito na consulta. Separada do repositório para deixar claro qual
/// caminho é leitura otimizada e qual é escrita transacional.
/// </summary>
public interface IConsultaDeImoveisResumidos
{
    /// <summary>
    /// Lista, busca e pagina os imóveis resumidos da organização do contexto,
    /// conforme <paramref name="filtro"/> (E2-F1-H2).
    /// Exemplo: <c>await consulta.ListarAsync(new FiltroDeListagemDeImoveis(null, null, null), cancelamento)</c>.
    /// </summary>
    Task<ResultadoPaginado<ImovelResumo>> ListarAsync(
        FiltroDeListagemDeImoveis filtro, CancellationToken cancelamento);
}
