using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Usuarios;

/// <summary>
/// Prova a confirmação de e-mail do cadastro contra o host real (E1-F2-H6): o
/// cadastro emite e envia o token, o link confirma sem exigir sessão, e o
/// reenvio invalida o token anterior — sem nunca bloquear o login.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class ConfirmacaoDeEmailTestes : TesteDeIntegracao
{
    public ConfirmacaoDeEmailTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Cadastro_emite_e_envia_um_token_de_confirmacao()
    {
        var email = await CadastrarAsync("Fenix Confirmacao");

        var envio = Assert.Single(Envios(), e => e.Email == email);
        Assert.NotEmpty(envio.Token);
    }

    [Fact]
    public async Task Token_valido_confirma_o_email_sem_exigir_sessao()
    {
        var email = await CadastrarAsync("Girassol Confirmacao");
        var token = UltimoTokenPara(email);

        var resposta = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/emails/verificacoes", new { email, token });

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.True(await NoBanco(banco =>
            banco.Users.Where(u => u.Email == email).Select(u => u.EmailConfirmed).SingleAsync()));
    }

    [Fact]
    public async Task Cadastro_nao_confirmado_ainda_consegue_logar()
    {
        var email = await CadastrarAsync("Horizonte Confirmacao");

        var entrada = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/sessoes", new { email, senha = SenhaDeTeste });

        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);
    }

    [Fact]
    public async Task Reenvio_invalida_o_token_anterior()
    {
        var email = await CadastrarAsync("Ipe Confirmacao");
        var tokenAntigo = UltimoTokenPara(email);
        var cliente = await ClienteAutenticado(email);

        var reenvio = await cliente.PostAsync("/emails/confirmacoes", content: null);
        Assert.Equal(HttpStatusCode.Accepted, reenvio.StatusCode);

        var confirmacaoComOAntigo = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/emails/verificacoes", new { email, token = tokenAntigo });
        Assert.Equal(HttpStatusCode.BadRequest, confirmacaoComOAntigo.StatusCode);

        var tokenNovo = UltimoTokenPara(email);
        var confirmacaoComONovo = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/emails/verificacoes", new { email, token = tokenNovo });
        Assert.Equal(HttpStatusCode.NoContent, confirmacaoComONovo.StatusCode);
    }

    [Fact]
    public async Task Reenvio_sem_sessao_recebe_401()
    {
        var resposta = await Ambiente.Aplicacao.CreateClient().PostAsync("/emails/confirmacoes", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_forjado_e_recusado()
    {
        var email = await CadastrarAsync("Jade Confirmacao");

        var resposta = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/emails/verificacoes", new { email, token = "token-inventado" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private async Task<string> CadastrarAsync(string nome)
    {
        var email = $"dono-{Guid.NewGuid():N}@exemplo.test";
        var resposta = await Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/contas", new
        {
            nomeCompleto = nome,
            email,
            senha = SenhaDeTeste,
            confirmacaoDeSenha = SenhaDeTeste,
        });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return email;
    }

    private async Task<HttpClient> ClienteAutenticado(string email)
    {
        var cliente = Ambiente.Aplicacao.CreateClient();
        var entrada = await cliente.PostAsJsonAsync("/sessoes", new { email, senha = SenhaDeTeste });
        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);
        return cliente;
    }

    private string UltimoTokenPara(string email) => Envios().Last(e => e.Email == email).Token;

    private List<EnviosDeEmailDeTeste.Envio> Envios()
        => Ambiente.Aplicacao.Services.GetRequiredService<EnviosDeEmailDeTeste>().Envios;
}
