using System.Net;
using System.Net.Http.Json;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Seguranca;

/// <summary>
/// Prova o limite de requisições por origem nas rotas de autenticação (E1-F2-H1).
/// Sobe um host próprio, apontado para o mesmo Postgres, com um teto apertado: a
/// suíte comum roda com teto folgado, e apertá-lo lá tornaria todo teste de login
/// refém da ordem de execução.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class LimiteDeAutenticacaoTestes : TesteDeIntegracao
{
    private const int TetoApertado = 3;

    public LimiteDeAutenticacaoTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Origem_que_ultrapassa_o_teto_recebe_429()
    {
        await using var hostApertado = new AplicacaoDeTeste(Ambiente.StringDeConexao, TetoApertado);
        var cliente = hostApertado.CreateClient();

        var statusPorTentativa = new List<HttpStatusCode>();
        for (var tentativa = 0; tentativa <= TetoApertado; tentativa++)
            statusPorTentativa.Add((await Entrar(cliente)).StatusCode);

        Assert.All(statusPorTentativa.Take(TetoApertado),
            status => Assert.Equal(HttpStatusCode.Unauthorized, status));
        Assert.Equal(HttpStatusCode.TooManyRequests, statusPorTentativa[TetoApertado]);
    }

    [Fact]
    public async Task Rota_autenticada_nao_entra_no_limite_das_rotas_de_autenticacao()
    {
        await using var hostApertado = new AplicacaoDeTeste(Ambiente.StringDeConexao, TetoApertado);
        var cliente = hostApertado.CreateClient();

        for (var tentativa = 0; tentativa < TetoApertado * 2; tentativa++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/imoveis")).StatusCode);
    }

    private static Task<HttpResponseMessage> Entrar(HttpClient cliente) =>
        cliente.PostAsJsonAsync("/sessoes", new
        {
            email = $"ninguem-{Guid.NewGuid():N}@exemplo.test",
            senha = "senha-qualquer-longa",
        });
}
