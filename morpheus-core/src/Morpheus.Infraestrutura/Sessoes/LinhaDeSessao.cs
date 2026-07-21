using Morpheus.Aplicacao.Sessoes;

namespace Morpheus.Infraestrutura.Sessoes;

/// <summary>
/// A linha de <c>sessoes</c> como o Npgsql a entrega: <c>timestamptz</c> vira
/// <see cref="DateTime"/> em UTC, não <see cref="DateTimeOffset"/>. Este tipo
/// existe só para receber essa forma e converter — sem ele, o Dapper não
/// materializa a <see cref="SessaoPersistida"/> e a falha só aparece em execução.
/// </summary>
internal sealed record LinhaDeSessao(Guid Id, Guid UsuarioId, byte[] Conteudo, DateTime ExpiraEm)
{
    public SessaoPersistida ParaSessao() =>
        new(Id, UsuarioId, Conteudo, new DateTimeOffset(DateTime.SpecifyKind(ExpiraEm, DateTimeKind.Utc)));
}
