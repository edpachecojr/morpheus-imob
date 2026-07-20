using System.Data;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Abre conexões de banco para as consultas Dapper. Interface fina de
/// propriedade do projeto sobre o driver Npgsql — o código de leitura não
/// conhece o SDK concreto de conexão.
/// </summary>
public interface IFabricaDeConexao
{
    Task<IDbConnection> AbrirAsync(CancellationToken cancelamento);
}
