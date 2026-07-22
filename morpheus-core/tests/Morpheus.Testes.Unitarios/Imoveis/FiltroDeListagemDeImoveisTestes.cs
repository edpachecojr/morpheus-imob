using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Imoveis;

namespace Morpheus.Testes.Unitarios.Imoveis;

public sealed class FiltroDeListagemDeImoveisTestes
{
    [Fact]
    public void Validar_aceita_filtro_sem_busca_nem_enum()
    {
        var filtro = new FiltroDeListagemDeImoveis(null, null, null);

        Assert.True(filtro.Validar().Sucesso);
    }

    [Fact]
    public void Validar_aceita_busca_e_enums_informados()
    {
        var filtro = new FiltroDeListagemDeImoveis("acácias", FinalidadeDoImovel.Locacao, SituacaoDoImovel.Disponivel);

        Assert.True(filtro.Validar().Sucesso);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_rejeita_pagina_menor_que_um(int pagina)
    {
        var filtro = new FiltroDeListagemDeImoveis(null, null, null, Pagina: pagina);

        var resultado = filtro.Validar();

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeListagemDeImoveis.PaginaInvalida(pagina), resultado.Erro);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validar_rejeita_tamanho_da_pagina_fora_do_intervalo(int tamanhoDaPagina)
    {
        var filtro = new FiltroDeListagemDeImoveis(null, null, null, TamanhoDaPagina: tamanhoDaPagina);

        var resultado = filtro.Validar();

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeListagemDeImoveis.TamanhoDaPaginaInvalido(tamanhoDaPagina), resultado.Erro);
    }

    [Fact]
    public void Validar_aceita_tamanho_da_pagina_no_limite_maximo()
    {
        var filtro = new FiltroDeListagemDeImoveis(
            null, null, null, TamanhoDaPagina: FiltroDeListagemDeImoveis.TamanhoMaximoDaPagina);

        Assert.True(filtro.Validar().Sucesso);
    }
}
