using Dapper;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Organizacoes;

/// <summary>
/// Busca o id da organização de um usuário na tabela de identidade, via Dapper em
/// conexão própria. NÃO usa o <see cref="MorpheusDbContext"/> de propósito: a
/// resolução do tenant é insumo do próprio contexto de dados (repositórios e filtro
/// de leitura dependem dela), então lê-la pelo mesmo <c>DbContext</c> acoplaria a
/// origem do tenant ao contexto que ela serve. Conexão separada mantém a resolução
/// independente do provedor de dados. É a fonte da verdade por trás do cache; roda
/// só no cache miss.
/// </summary>
public sealed class ConsultaDaOrganizacaoDoUsuarioComDapper : IConsultaDaOrganizacaoDoUsuario
{
    // Tabela e colunas seguem a convenção snake_case do schema; o mapeamento do
    // store do Identity para "usuarios" está em ConfiguracaoDeUsuarioDaOrganizacao.
    private const string BuscarOrganizacaoDoUsuario =
        "SELECT organizacao_id FROM usuarios WHERE id = @UsuarioId";

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
