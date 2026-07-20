using Morpheus.Dominio.Imoveis;

namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Escrita e leitura transacional de imóveis via EF Core, sempre restritas à
/// organização do contexto. É o lado de escrita do modelo; leitura performática
/// fica em <see cref="IConsultaDeImoveisResumidos"/>.
/// </summary>
public interface IRepositorioDeImoveis
{
    Task AdicionarAsync(Imovel imovel, CancellationToken cancelamento);

    Task<IReadOnlyList<Imovel>> ListarDaOrganizacaoAsync(CancellationToken cancelamento);

    Task<Imovel?> BuscarPorCodigoAsync(string codigoDeReferencia, CancellationToken cancelamento);
}
