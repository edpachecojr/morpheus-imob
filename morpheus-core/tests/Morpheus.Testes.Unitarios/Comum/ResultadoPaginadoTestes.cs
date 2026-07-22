using Morpheus.Aplicacao.Comum;

namespace Morpheus.Testes.Unitarios.Comum;

public sealed class ResultadoPaginadoTestes
{
    [Fact]
    public void TotalDePaginas_arredonda_para_cima_quando_ha_resto()
    {
        var resultado = new ResultadoPaginado<string>(["a"], Total: 21, Pagina: 1, TamanhoDaPagina: 20);

        Assert.Equal(2, resultado.TotalDePaginas);
    }

    [Fact]
    public void TotalDePaginas_fecha_exato_quando_nao_ha_resto()
    {
        var resultado = new ResultadoPaginado<string>(["a"], Total: 40, Pagina: 1, TamanhoDaPagina: 20);

        Assert.Equal(2, resultado.TotalDePaginas);
    }

    [Fact]
    public void TotalDePaginas_e_zero_quando_nao_ha_resultado()
    {
        var resultado = new ResultadoPaginado<string>([], Total: 0, Pagina: 1, TamanhoDaPagina: 20);

        Assert.Equal(0, resultado.TotalDePaginas);
    }
}
