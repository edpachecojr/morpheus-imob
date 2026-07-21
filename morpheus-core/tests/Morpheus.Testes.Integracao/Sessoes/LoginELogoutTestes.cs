using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Sessoes;

/// <summary>
/// Prova o login por e-mail e senha e o encerramento de sessão (E1-F2-H1 e
/// E1-F2-H4) ponta a ponta: cookie opaco emitido, sessão gravada no servidor,
/// acesso liberado enquanto ela existe e negado na requisição seguinte ao logout.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class LoginELogoutTestes : TesteDeIntegracao
{
    public LoginELogoutTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Credenciais_validas_emitem_cookie_de_sessao_opaco()
    {
        var conta = await SemearOrganizacaoAsync("Aurora Login");
        var cliente = Ambiente.Aplicacao.CreateClient();

        var resposta = await Entrar(cliente, conta.EmailDoDono, SenhaDeTeste);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var cookie = Assert.Single(resposta.Headers.GetValues("Set-Cookie"));
        Assert.Contains("morpheus_sessao=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Sessao_valida_e_gravada_no_servidor_vinculada_ao_usuario()
    {
        var conta = await SemearOrganizacaoAsync("Bela Vista Login");
        var cliente = Ambiente.Aplicacao.CreateClient();

        await Entrar(cliente, conta.EmailDoDono, SenhaDeTeste);

        var sessoes = await NoBanco(banco =>
            banco.Sessoes.CountAsync(sessao => sessao.UsuarioId == conta.UsuarioId));
        Assert.Equal(1, sessoes);
    }

    [Fact]
    public async Task Sessao_aberta_da_acesso_a_rota_autenticada_da_propria_organizacao()
    {
        var conta = await SemearOrganizacaoAsync("Cristal Login");
        await SemearImovelAsync(conta.OrganizacaoId, "AP-LOGIN-1", "Rua do Login, 1");
        var cliente = Ambiente.Aplicacao.CreateClient();
        await Entrar(cliente, conta.EmailDoDono, SenhaDeTeste);

        var resposta = await cliente.GetAsync("/imoveis");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("AP-LOGIN-1", await resposta.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Logout_revoga_a_sessao_no_servidor_e_a_requisicao_seguinte_recebe_401()
    {
        var conta = await SemearOrganizacaoAsync("Delta Login");
        var cliente = Ambiente.Aplicacao.CreateClient();
        await Entrar(cliente, conta.EmailDoDono, SenhaDeTeste);

        var saida = await cliente.DeleteAsync("/sessoes/atual");
        var depois = await cliente.GetAsync("/imoveis");

        Assert.Equal(HttpStatusCode.NoContent, saida.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, depois.StatusCode);
        Assert.Equal(0, await NoBanco(banco =>
            banco.Sessoes.CountAsync(sessao => sessao.UsuarioId == conta.UsuarioId)));
    }

    [Fact]
    public async Task Logout_derruba_apenas_o_aparelho_que_saiu()
    {
        var conta = await SemearOrganizacaoAsync("Eco Login");
        var aparelhoA = Ambiente.Aplicacao.CreateClient();
        var aparelhoB = Ambiente.Aplicacao.CreateClient();
        await Entrar(aparelhoA, conta.EmailDoDono, SenhaDeTeste);
        await Entrar(aparelhoB, conta.EmailDoDono, SenhaDeTeste);

        await aparelhoA.DeleteAsync("/sessoes/atual");

        Assert.Equal(HttpStatusCode.Unauthorized, (await aparelhoA.GetAsync("/imoveis")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await aparelhoB.GetAsync("/imoveis")).StatusCode);
    }

    [Fact]
    public async Task Logout_sem_sessao_recebe_401()
        => Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await Ambiente.Aplicacao.CreateClient().DeleteAsync("/sessoes/atual")).StatusCode);

    private static Task<HttpResponseMessage> Entrar(HttpClient cliente, string email, string senha)
        => cliente.PostAsJsonAsync("/sessoes", new { email, senha });
}
