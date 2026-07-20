namespace Morpheus.Infraestrutura.Imoveis;

/// <summary>
/// Monta o SQL de leitura de imóveis com o filtro de organização SEMPRE
/// explícito no WHERE. Isolar a montagem em texto puro tornou o filtro testável
/// sem banco: o teste prova que nenhuma consulta sai sem <c>organizacao_id</c>.
/// </summary>
public static class ConsultaDeImoveisDaOrganizacao
{
    public const string ParametroOrganizacao = "OrganizacaoId";

    public static string ListarResumoPorOrganizacao() =>
        """
        SELECT id AS "Id",
               codigo_de_referencia AS "CodigoDeReferencia",
               endereco AS "Endereco"
        FROM imoveis
        WHERE organizacao_id = @OrganizacaoId
        ORDER BY codigo_de_referencia;
        """;
}
