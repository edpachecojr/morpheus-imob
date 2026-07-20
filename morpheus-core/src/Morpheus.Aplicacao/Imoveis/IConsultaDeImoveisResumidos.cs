namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Leitura performática de imóveis via Dapper, com o filtro de organização
/// sempre explícito na consulta. Separada do repositório para deixar claro qual
/// caminho é leitura otimizada e qual é escrita transacional.
/// </summary>
public interface IConsultaDeImoveisResumidos
{
    Task<IReadOnlyList<ImovelResumo>> ListarAsync(CancellationToken cancelamento);
}
