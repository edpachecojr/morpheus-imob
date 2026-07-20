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
}
