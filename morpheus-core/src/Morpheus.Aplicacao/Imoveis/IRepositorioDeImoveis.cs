using Morpheus.Dominio.Imoveis;

namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Escrita e leitura transacional de imóveis via EF Core, sempre restritas à
/// organização do contexto. É o lado de escrita do modelo; leitura performática
/// fica em <see cref="IConsultaDeImoveisResumidos"/>.
/// </summary>
public interface IRepositorioDeImoveis
{
    /// <summary>
    /// Persiste um imóvel novo na organização do contexto.
    /// Exemplo: <c>await repositorio.AdicionarAsync(imovel, cancelamento)</c>.
    /// </summary>
    Task AdicionarAsync(Imovel imovel, CancellationToken cancelamento);

    /// <summary>
    /// Lista todos os imóveis da organização do contexto.
    /// Exemplo: <c>var imoveis = await repositorio.ListarDaOrganizacaoAsync(cancelamento)</c>.
    /// </summary>
    Task<IReadOnlyList<Imovel>> ListarDaOrganizacaoAsync(CancellationToken cancelamento);

    /// <summary>
    /// Busca um imóvel pelo código de referência, restrito à organização do
    /// contexto. Devolve <c>null</c> quando não existe.
    /// Exemplo: <c>await repositorio.BuscarPorCodigoAsync("AP-101", cancelamento)</c>.
    /// </summary>
    Task<Imovel?> BuscarPorCodigoAsync(string codigoDeReferencia, CancellationToken cancelamento);
}
