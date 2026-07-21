namespace Morpheus.Infraestrutura.Sessoes;

/// <summary>
/// SQL da tabela de sessões, isolado em texto puro para ficar legível e revisável
/// sem abrir o adaptador. A busca filtra por validade na própria consulta: sessão
/// expirada é indistinguível de sessão inexistente, e não depende de ninguém ter
/// rodado uma limpeza antes.
/// </summary>
internal static class ComandosDeSessao
{
    public const string Guardar =
        """
        INSERT INTO sessoes (id, usuario_id, conteudo, expira_em, criada_em)
        VALUES (@Id, @UsuarioId, @Conteudo, @ExpiraEm, @CriadaEm);
        """;

    public const string Renovar =
        """
        UPDATE sessoes
        SET conteudo = @Conteudo, expira_em = @ExpiraEm
        WHERE id = @Id;
        """;

    public const string Buscar =
        """
        SELECT id AS "Id",
               usuario_id AS "UsuarioId",
               conteudo AS "Conteudo",
               expira_em AS "ExpiraEm"
        FROM sessoes
        WHERE id = @Id AND expira_em > @Agora;
        """;

    public const string Remover = "DELETE FROM sessoes WHERE id = @Id;";

    public const string RemoverDoUsuario = "DELETE FROM sessoes WHERE usuario_id = @UsuarioId;";
}
