using System.Reflection;
using Morpheus.Api.Endpoints;

namespace Morpheus.Testes.Unitarios.Autorizacao;

/// <summary>
/// Prova que <c>tenant_id</c> enviado no corpo é ignorado (E1-F3-H1) da forma mais
/// forte possível: <b>nenhum</b> contrato de requisição da API tem campo de
/// organização ou tenant para ligar. Ignorar em tempo de execução dependeria de
/// alguém lembrar; não existir o campo é estrutural.
/// </summary>
public sealed class ContratosDeRequisicaoTestes
{
    private static readonly string[] TermosProibidos = ["organizacao", "tenant", "papel", "permissao"];

    [Fact]
    public void Nenhuma_requisicao_da_api_aceita_organizacao_papel_ou_permissao()
    {
        var infracoes = ContratosDeRequisicao()
            .SelectMany(contrato => contrato.GetProperties().Select(campo => (contrato, campo)))
            .Where(par => TermosProibidos.Any(termo =>
                par.campo.Name.Contains(termo, StringComparison.OrdinalIgnoreCase)))
            .Select(par => $"{par.contrato.Name}.{par.campo.Name}")
            .ToList();

        Assert.Empty(infracoes);
    }

    [Fact]
    public void A_varredura_encontra_os_contratos_de_requisicao()
    {
        // Sem esta guarda, o teste acima passaria vazio se os contratos mudassem de
        // convenção de nome — e deixaria de proteger qualquer coisa.
        Assert.NotEmpty(ContratosDeRequisicao());
    }

    private static List<Type> ContratosDeRequisicao() =>
        [.. typeof(IEndpoint).Assembly.GetTypes()
            .Where(tipo => tipo.IsClass && tipo.IsPublic)
            .Where(tipo => tipo.Name.StartsWith("Requisicao", StringComparison.Ordinal))];
}
