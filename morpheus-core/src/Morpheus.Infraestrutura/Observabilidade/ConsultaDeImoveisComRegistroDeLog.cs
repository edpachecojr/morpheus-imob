using Microsoft.Extensions.Logging;
using Morpheus.Aplicacao.Comum;
using Morpheus.Aplicacao.Imoveis;

namespace Morpheus.Infraestrutura.Observabilidade;

/// <summary>
/// Decorador de <see cref="IConsultaDeImoveisResumidos"/> que registra a execução
/// da listagem (duração e desfecho) sem tocar no leitor Dapper. É o exemplo de
/// referência do padrão: log transversal adicionado por composição, não por
/// edição do serviço (OCP). Serviços novos aderem via
/// <see cref="DecoracaoDeServico.Decorar{TServico, TDecorador}"/>.
/// </summary>
public sealed class ConsultaDeImoveisComRegistroDeLog : IConsultaDeImoveisResumidos
{
    private readonly IConsultaDeImoveisResumidos _interno;
    private readonly ILogger<ConsultaDeImoveisComRegistroDeLog> _diario;

    public ConsultaDeImoveisComRegistroDeLog(
        IConsultaDeImoveisResumidos interno,
        ILogger<ConsultaDeImoveisComRegistroDeLog> diario)
    {
        _interno = interno;
        _diario = diario;
    }

    public Task<ResultadoPaginado<ImovelResumo>> ListarAsync(
        FiltroDeListagemDeImoveis filtro, CancellationToken cancelamento) =>
        RegistroDeExecucaoDeServico.MedirAsync(
            _diario, "listar_imoveis_resumidos", () => _interno.ListarAsync(filtro, cancelamento));
}
