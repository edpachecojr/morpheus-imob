namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Leitura performática de imóveis via Dapper, com o filtro de organização
/// sempre explícito na consulta. Separada do repositório para deixar claro qual
/// caminho é leitura otimizada e qual é escrita transacional.
/// </summary>
public interface IConsultaDeImoveisResumidos
{
    /// <summary>
    /// Lista os imóveis resumidos da organização do contexto.
    /// Exemplo: <c>await consulta.ListarAsync(cancelamento)</c>.
    /// </summary>
    Task<IReadOnlyList<ImovelResumo>> ListarAsync(CancellationToken cancelamento);
}
