using Morpheus.Api.Endpoints.Imoveis;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Imoveis;

namespace Morpheus.Testes.Unitarios.Imoveis;

public sealed class ParametrosDeListagemDeImoveisTestes
{
    [Fact]
    public void ParaFiltro_converte_finalidade_e_situacao_informadas()
    {
        var parametros = new ParametrosDeListagemDeImoveis
        {
            Busca = "acácias",
            Finalidade = "Locacao",
            Situacao = "Disponivel",
            Pagina = 2,
            TamanhoDaPagina = 10,
        };

        var resultado = parametros.ParaFiltro();

        Assert.True(resultado.Sucesso);
        Assert.Equal(new FiltroDeListagemDeImoveis("acácias", FinalidadeDoImovel.Locacao, SituacaoDoImovel.Disponivel, 2, 10), resultado.Valor);
    }

    [Fact]
    public void ParaFiltro_converte_nome_do_enum_ignorando_maiusculas()
    {
        var parametros = new ParametrosDeListagemDeImoveis { Finalidade = "vEnDa" };

        var resultado = parametros.ParaFiltro();

        Assert.Equal(FinalidadeDoImovel.Venda, resultado.Valor.Finalidade);
    }

    [Fact]
    public void ParaFiltro_sem_finalidade_nem_situacao_nao_restringe_o_filtro()
    {
        var parametros = new ParametrosDeListagemDeImoveis();

        var resultado = parametros.ParaFiltro();

        Assert.True(resultado.Sucesso);
        Assert.Null(resultado.Valor.Finalidade);
        Assert.Null(resultado.Valor.Situacao);
    }

    [Fact]
    public void ParaFiltro_rejeita_finalidade_desconhecida_citando_o_valor_recebido()
    {
        var parametros = new ParametrosDeListagemDeImoveis { Finalidade = "aluguel-temporada" };

        var resultado = parametros.ParaFiltro();

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeListagemDeImoveis.FinalidadeInvalida("aluguel-temporada"), resultado.Erro);
    }

    [Fact]
    public void ParaFiltro_rejeita_situacao_desconhecida_citando_o_valor_recebido()
    {
        var parametros = new ParametrosDeListagemDeImoveis { Situacao = "vendido" };

        var resultado = parametros.ParaFiltro();

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeListagemDeImoveis.SituacaoInvalida("vendido"), resultado.Erro);
    }
}
