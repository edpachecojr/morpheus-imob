using System.Net;
using System.Net.Http.Json;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Autorizacao;

/// <summary>
/// Testes negativos de autorização (E1-F3-H1 e E1-F3-H2) contra o host real: sem
/// sessão é 401, com sessão mas sem permissão é 403, e a mesma rota responde 200
/// para quem tem o papel certo. É o ponto em que a maioria dos sistemas falha,
/// porque só se testa o caminho feliz.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class AutorizacaoDeRotasTestes : TesteDeIntegracao
{
    public AutorizacaoDeRotasTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Theory]
    [InlineData("/imoveis")]
    [InlineData("/organizacao/usuarios")]
    public async Task Rota_autenticada_sem_sessao_recebe_401(string rota)
        => Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await Ambiente.Aplicacao.CreateClient().GetAsync(rota)).StatusCode);

    [Fact]
    public async Task Corretor_recebe_403_em_rota_que_exige_usuario_gerenciar()
    {
        var organizacao = await SemearOrganizacaoAsync("Aurora Autorizacao");
        var corretor = await SemearUsuarioAsync(
            organizacao.OrganizacaoId, PapeisDoUsuario.Corretor, "Corretor Aurora");
        var cliente = await ClienteAutenticado(corretor.Email);

        var resposta = await cliente.GetAsync("/organizacao/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Dono_recebe_200_na_mesma_rota_que_nega_o_corretor()
    {
        var organizacao = await SemearOrganizacaoAsync("Bela Vista Autorizacao");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetAsync("/organizacao/usuarios");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Corretor_acessa_o_que_a_matriz_concede_a_ele()
    {
        var organizacao = await SemearOrganizacaoAsync("Cristal Autorizacao");
        var corretor = await SemearUsuarioAsync(
            organizacao.OrganizacaoId, PapeisDoUsuario.Corretor, "Corretor Cristal");
        var cliente = await ClienteAutenticado(corretor.Email);

        var resposta = await cliente.GetAsync("/imoveis");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Listagem_de_usuarios_traz_apenas_a_equipe_da_organizacao_do_contexto()
    {
        var aurora = await SemearOrganizacaoAsync("Delta Autorizacao");
        var outra = await SemearOrganizacaoAsync("Eco Autorizacao");
        var cliente = await ClienteAutenticado(aurora.EmailDoDono);

        var corpo = await (await cliente.GetAsync("/organizacao/usuarios")).Content.ReadAsStringAsync();

        Assert.Contains(aurora.EmailDoDono, corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(outra.EmailDoDono, corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Saude_continua_anonima()
        => Assert.Equal(
            HttpStatusCode.OK,
            (await Ambiente.Aplicacao.CreateClient().GetAsync("/health")).StatusCode);

    private async Task<HttpClient> ClienteAutenticado(string email)
    {
        var cliente = Ambiente.Aplicacao.CreateClient();
        var entrada = await cliente.PostAsJsonAsync("/sessoes", new { email, senha = SenhaDeTeste });
        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);
        return cliente;
    }
}
