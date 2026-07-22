using Morpheus.Aplicacao.Comum;
using Morpheus.Aplicacao.Imoveis;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Consulta de imóveis falsa: devolve uma página fixa e conta as chamadas, para
/// provar que o decorador delega ao serviço interno sem alterar o resultado.
/// Pode ser configurada para lançar, exercitando o caminho de falha do log.
/// </summary>
public sealed class ConsultaDeImoveisResumidosFake : IConsultaDeImoveisResumidos
{
    private readonly ResultadoPaginado<ImovelResumo> _resposta;
    private readonly Exception? _falha;

    public int Chamadas { get; private set; }
    public FiltroDeListagemDeImoveis? UltimoFiltroRecebido { get; private set; }

    private ConsultaDeImoveisResumidosFake(ResultadoPaginado<ImovelResumo> resposta, Exception? falha)
    {
        _resposta = resposta;
        _falha = falha;
    }

    public static ConsultaDeImoveisResumidosFake QueRetorna(params ImovelResumo[] imoveis) =>
        new(new ResultadoPaginado<ImovelResumo>(imoveis, imoveis.Length, 1, imoveis.Length == 0 ? 20 : imoveis.Length), null);

    public static ConsultaDeImoveisResumidosFake QueFalhaCom(Exception falha) =>
        new(new ResultadoPaginado<ImovelResumo>([], 0, 1, 20), falha);

    public Task<ResultadoPaginado<ImovelResumo>> ListarAsync(
        FiltroDeListagemDeImoveis filtro, CancellationToken cancelamento)
    {
        Chamadas++;
        UltimoFiltroRecebido = filtro;
        return _falha is not null
            ? Task.FromException<ResultadoPaginado<ImovelResumo>>(_falha)
            : Task.FromResult(_resposta);
    }
}
