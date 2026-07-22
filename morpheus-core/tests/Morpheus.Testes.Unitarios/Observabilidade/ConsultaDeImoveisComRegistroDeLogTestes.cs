using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Imoveis;
using Morpheus.Infraestrutura.Observabilidade;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Observabilidade;

/// <summary>
/// Prova que o decorador de log da consulta de imóveis delega ao leitor interno e
/// devolve exatamente o resultado dele — o log é transparente ao comportamento.
/// </summary>
public sealed class ConsultaDeImoveisComRegistroDeLogTestes
{
    private static readonly FiltroDeListagemDeImoveis SemFiltro = new(null, null, null);

    [Fact]
    public async Task Delega_ao_interno_e_devolve_o_mesmo_resultado()
    {
        var imovel = new ImovelResumo(
            Guid.NewGuid(), "AP-101", "Título qualquer", FinalidadeDoImovel.Locacao,
            SituacaoDoImovel.Disponivel, "Rua das Flores, 100");
        var interno = ConsultaDeImoveisResumidosFake.QueRetorna(imovel);
        var decorado = new ConsultaDeImoveisComRegistroDeLog(interno, new DiarioDeOperacoesFake<ConsultaDeImoveisComRegistroDeLog>());

        var resultado = await decorado.ListarAsync(SemFiltro, CancellationToken.None);

        Assert.Equal(1, interno.Chamadas);
        Assert.Same(imovel, Assert.Single(resultado.Itens));
    }

    [Fact]
    public async Task Propaga_a_falha_do_interno()
    {
        var interno = ConsultaDeImoveisResumidosFake.QueFalhaCom(new InvalidOperationException("leitura falhou"));
        var decorado = new ConsultaDeImoveisComRegistroDeLog(interno, new DiarioDeOperacoesFake<ConsultaDeImoveisComRegistroDeLog>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => decorado.ListarAsync(SemFiltro, CancellationToken.None));
    }
}
