using Dapper;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Organizacoes;

/// <summary>
/// Busca o id da organização de um usuário na tabela de identidade, via Dapper em
/// conexão própria. NÃO usa o <see cref="MorpheusDbContext"/> de propósito: o
/// interceptor de escrita depende (transitivamente) desta consulta, e lê-la pelo
/// mesmo contexto criaria dependência circular na composição — o contexto depende
/// do interceptor, que dependeria do contexto — além de consulta reentrante no
/// meio do SaveChanges. Conexão separada resolve os dois. É a fonte da verdade por
/// trás do cache; roda só no cache miss.
/// </summary>
public sealed class ConsultaDaOrganizacaoDoUsuarioComDapper : IConsultaDaOrganizacaoDoUsuario
{
    // A tabela do IdentityCore mantém o nome PascalCase (exige aspas no Postgres);
    // as colunas seguem a convenção snake_case do restante do schema.
    private const string BuscarOrganizacaoDoUsuario =
        """SELECT organizacao_id FROM "AspNetUsers" WHERE id = @UsuarioId""";

    private readonly IFabricaDeConexao _fabrica;

    public ConsultaDaOrganizacaoDoUsuarioComDapper(IFabricaDeConexao fabrica) => _fabrica = fabrica;

    public async Task<Guid?> BuscarOrganizacaoIdAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        using var conexao = await _fabrica.AbrirAsync(cancelamento);
        var comando = new CommandDefinition(
            BuscarOrganizacaoDoUsuario,
            new { UsuarioId = usuarioId },
            cancellationToken: cancelamento);
        return await conexao.QuerySingleOrDefaultAsync<Guid?>(comando);
    }
}
