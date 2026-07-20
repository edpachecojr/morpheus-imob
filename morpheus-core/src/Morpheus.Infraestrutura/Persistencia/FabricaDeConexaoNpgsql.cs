using System.Data;
using Npgsql;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Abre conexões PostgreSQL abertas e prontas para uso pelo Dapper. Recebe a
/// string de conexão por construtor — nunca lê configuração global no ponto de uso.
/// </summary>
public sealed class FabricaDeConexaoNpgsql : IFabricaDeConexao
{
    private readonly string _stringDeConexao;

    public FabricaDeConexaoNpgsql(string stringDeConexao)
    {
        if (string.IsNullOrWhiteSpace(stringDeConexao))
            throw new ArgumentException("String de conexão não pode ser vazia.", nameof(stringDeConexao));
        _stringDeConexao = stringDeConexao;
    }

    public async Task<IDbConnection> AbrirAsync(CancellationToken cancelamento)
    {
        var conexao = new NpgsqlConnection(_stringDeConexao);
        await conexao.OpenAsync(cancelamento);
        return conexao;
    }
}
