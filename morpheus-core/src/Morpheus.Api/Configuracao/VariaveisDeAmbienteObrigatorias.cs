namespace Morpheus.Api.Configuracao;

/// <summary>
/// Lê a configuração obrigatória na subida da aplicação. A ausência de uma
/// variável derruba o processo com mensagem que diz QUAL variável falta e o
/// formato esperado — falhar cedo evita subir num estado inutilizável (E1-F0-H2).
/// </summary>
public static class VariaveisDeAmbienteObrigatorias
{
    public const string ChaveDaConexao = "MORPHEUS_BANCO_CONEXAO";

    public static string LerStringDeConexao(IConfiguration configuracao)
    {
        var conexao = configuracao[ChaveDaConexao];
        if (string.IsNullOrWhiteSpace(conexao))
            throw new InvalidOperationException(
                $"Variável de ambiente obrigatória ausente: {ChaveDaConexao}. " +
                "Formato esperado: 'Host=<host>;Port=5432;Database=<db>;Username=<user>;Password=<senha>'.");
        return conexao;
    }
}
