using System.Net;
using System.Net.Http.Json;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Sessoes;

/// <summary>
/// Prova que senha errada, e-mail inexistente e conta bloqueada devolvem a
/// <b>mesma</b> resposta — exigência de fundacao/autenticacao.md. Compara corpo e
/// status, não tempo: a igualdade de tempo é garantida pela derivação de chave em
/// todos os caminhos, provada por unidade em <c>AutenticacaoDeUsuarioTestes</c>,
/// que é determinística onde uma medição de relógio seria instável.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class RespostaGenericaDeLoginTestes : TesteDeIntegracao
{
    private const string SenhaErrada = "senha-errada-mas-longa";

    public RespostaGenericaDeLoginTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Senha_errada_e_email_inexistente_respondem_identicamente()
    {
        var conta = await SemearOrganizacaoAsync("Aurora Recusa");

        var comSenhaErrada = await Entrar(conta.EmailDoDono, SenhaErrada);
        var semConta = await Entrar($"ninguem-{Guid.NewGuid():N}@exemplo.test", SenhaErrada);

        Assert.Equal(HttpStatusCode.Unauthorized, comSenhaErrada.Status);
        Assert.Equal(comSenhaErrada, semConta);
    }

    [Fact]
    public async Task Conta_bloqueada_responde_igual_as_demais_recusas()
    {
        var conta = await SemearOrganizacaoAsync("Bela Vista Recusa");
        var semConta = await Entrar($"ninguem-{Guid.NewGuid():N}@exemplo.test", SenhaErrada);

        // Cinco erros bloqueiam a conta (IdentityOptions.Lockout).
        for (var tentativa = 0; tentativa < 5; tentativa++)
            await Entrar(conta.EmailDoDono, SenhaErrada);

        var bloqueada = await Entrar(conta.EmailDoDono, SenhaDeTeste);
        Assert.Equal(semConta, bloqueada);
    }

    [Fact]
    public async Task Recusa_nao_emite_cookie_de_sessao()
    {
        var conta = await SemearOrganizacaoAsync("Cristal Recusa");

        var resposta = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/sessoes", new { email = conta.EmailDoDono, senha = SenhaErrada });

        Assert.False(resposta.Headers.Contains("Set-Cookie"));
    }

    private async Task<RecusaObservavel> Entrar(string email, string senha)
    {
        var resposta = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/sessoes", new { email, senha });
        return new RecusaObservavel(resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
    }

    /// <summary>Tudo que um cliente consegue observar de uma recusa de login.</summary>
    private sealed record RecusaObservavel(HttpStatusCode Status, string Corpo);
}
