namespace Morpheus.Infraestrutura.Sessoes;

/// <summary>
/// Linha da tabela <c>sessoes</c>. Existe para que o schema da sessão viva no
/// modelo do EF e entre nas migrações como qualquer outra tabela — a leitura e a
/// escrita em si passam por Dapper
/// (<see cref="ArmazenamentoDeSessoesComDapper"/>), porque rodam a cada
/// requisição, fora do escopo de um <c>DbContext</c>.
/// <para>
/// Não é entidade de domínio e não pertence a uma organização: o tenant do usuário
/// é resolvido depois que a sessão já foi restaurada, e vincular a sessão a ele
/// criaria um ciclo.
/// </para>
/// </summary>
public sealed class SessaoDoPainelArmazenada
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public byte[] Conteudo { get; private set; } = [];
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }
}
