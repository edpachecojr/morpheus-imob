namespace Morpheus.Infraestrutura.Imoveis;

/// <summary>
/// Monta o SQL de leitura de imóveis com o filtro de organização SEMPRE
/// explícito no WHERE. Isolar a montagem em texto puro tornou o filtro testável
/// sem banco: o teste prova que nenhuma consulta sai sem <c>organizacao_id</c>.
/// Busca, finalidade, situação e paginação (E2-F1-H2) entram como parâmetros
/// opcionais — presença nula não restringe o resultado.
/// </summary>
public static class ConsultaDeImoveisDaOrganizacao
{
    public const string ParametroOrganizacao = "OrganizacaoId";

    public static string ListarResumoPorOrganizacao() =>
        """
        SELECT id AS "Id",
               codigo_de_referencia AS "CodigoDeReferencia",
               titulo AS "Titulo",
               finalidade AS "Finalidade",
               situacao AS "Situacao",
               endereco AS "Endereco",
               CAST(COUNT(*) OVER() AS INTEGER) AS "Total"
        FROM imoveis
        WHERE organizacao_id = @OrganizacaoId
          AND (@Busca::text IS NULL
               OR codigo_de_referencia ILIKE '%' || @Busca::text || '%'
               OR titulo ILIKE '%' || @Busca::text || '%'
               OR endereco ILIKE '%' || @Busca::text || '%')
          AND (@Finalidade::text IS NULL OR finalidade = @Finalidade::text)
          AND (@Situacao::text IS NULL OR situacao = @Situacao::text)
        ORDER BY codigo_de_referencia
        LIMIT @TamanhoDaPagina OFFSET @Offset;
        """;
}
