using Morpheus.Aplicacao.Imoveis;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Consulta de imóveis falsa: devolve uma lista fixa e conta as chamadas, para
/// provar que o decorador delega ao serviço interno sem alterar o resultado.
/// Pode ser configurada para lançar, exercitando o caminho de falha do log.
/// </summary>
public sealed class ConsultaDeImoveisResumidosFake : IConsultaDeImoveisResumidos
{
    private readonly IReadOnlyList<ImovelResumo> _resposta;
    private readonly Exception? _falha;

    public int Chamadas { get; private set; }

    private ConsultaDeImoveisResumidosFake(IReadOnlyList<ImovelResumo> resposta, Exception? falha)
    {
        _resposta = resposta;
        _falha = falha;
    }

    public static ConsultaDeImoveisResumidosFake QueRetorna(params ImovelResumo[] imoveis) =>
        new(imoveis, null);

    public static ConsultaDeImoveisResumidosFake QueFalhaCom(Exception falha) =>
        new([], falha);

    public Task<IReadOnlyList<ImovelResumo>> ListarAsync(CancellationToken cancelamento)
    {
        Chamadas++;
        return _falha is not null
            ? Task.FromException<IReadOnlyList<ImovelResumo>>(_falha)
            : Task.FromResult(_resposta);
    }
}
