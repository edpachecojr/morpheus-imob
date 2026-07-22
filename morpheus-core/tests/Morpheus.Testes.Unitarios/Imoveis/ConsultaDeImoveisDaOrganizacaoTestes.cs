using Morpheus.Infraestrutura.Imoveis;

namespace Morpheus.Testes.Unitarios.Imoveis;

public sealed class ConsultaDeImoveisDaOrganizacaoTestes
{
    [Fact]
    public void Sql_de_listagem_filtra_explicitamente_por_organizacao()
    {
        var sql = ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao();

        Assert.Contains($"organizacao_id = @{ConsultaDeImoveisDaOrganizacao.ParametroOrganizacao}", sql);
    }

    [Fact]
    public void Sql_de_listagem_le_da_tabela_imoveis()
    {
        var sql = ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao();

        Assert.Contains("FROM imoveis", sql);
    }

    [Fact]
    public void Sql_de_listagem_busca_por_codigo_titulo_e_endereco()
    {
        var sql = ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao();

        Assert.Contains("codigo_de_referencia ILIKE '%' || @Busca::text || '%'", sql);
        Assert.Contains("titulo ILIKE '%' || @Busca::text || '%'", sql);
        Assert.Contains("endereco ILIKE '%' || @Busca::text || '%'", sql);
    }

    [Fact]
    public void Sql_de_listagem_filtra_por_finalidade_e_situacao_quando_informados()
    {
        var sql = ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao();

        Assert.Contains("@Finalidade::text IS NULL OR finalidade = @Finalidade::text", sql);
        Assert.Contains("@Situacao::text IS NULL OR situacao = @Situacao::text", sql);
    }

    [Fact]
    public void Sql_de_listagem_pagina_com_limit_e_offset()
    {
        var sql = ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao();

        Assert.Contains("LIMIT @TamanhoDaPagina OFFSET @Offset", sql);
    }

    [Fact]
    public void Sql_de_listagem_traz_o_total_da_janela_para_paginacao()
    {
        var sql = ConsultaDeImoveisDaOrganizacao.ListarResumoPorOrganizacao();

        Assert.Contains("CAST(COUNT(*) OVER() AS INTEGER) AS \"Total\"", sql);
    }
}
