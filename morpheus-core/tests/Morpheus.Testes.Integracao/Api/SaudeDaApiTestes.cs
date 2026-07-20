using System.Net.Http.Json;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Api;

/// <summary>
/// Prova, pela porta HTTP real, que o host sobe com a configuração de teste e
/// responde <c>/health</c> — o contrato de prontidão da Fase 0 (E1-F0-H2).
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class SaudeDaApiTestes
{
    private readonly AmbienteDeIntegracao _ambiente;

    public SaudeDaApiTestes(AmbienteDeIntegracao ambiente) => _ambiente = ambiente;

    [Fact]
    public async Task Get_health_responde_status_ok()
    {
        var cliente = _ambiente.Aplicacao.CreateClient();

        var resposta = await cliente.GetAsync("/health");

        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaDeSaude>();
        Assert.Equal("ok", corpo?.Status);
    }

    private sealed record RespostaDeSaude(string Status);
}
