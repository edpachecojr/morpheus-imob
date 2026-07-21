namespace Morpheus.Aplicacao.Sessoes;

/// <summary>
/// Uma sessão do painel guardada no servidor. O <see cref="Conteudo"/> é opaco de
/// propósito — quem armazena não interpreta o que há dentro, só sabe a quem
/// pertence e até quando vale, que é o suficiente para revogar.
/// </summary>
public sealed record SessaoPersistida(
    Guid Id, Guid UsuarioId, byte[] Conteudo, DateTimeOffset ExpiraEm);
