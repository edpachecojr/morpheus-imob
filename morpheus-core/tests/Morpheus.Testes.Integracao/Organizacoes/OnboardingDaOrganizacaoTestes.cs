using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Organizacoes;

/// <summary>
/// Prova o onboarding da organização contra o host real (E1-F1-H4): o dono
/// renomeia, o corretor não pode, e o novo nome é o que qualquer leitura
/// subsequente enxerga — sem exigir nova implantação.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class OnboardingDaOrganizacaoTestes : TesteDeIntegracao
{
    public OnboardingDaOrganizacaoTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Dono_renomeia_a_organizacao()
    {
        var conta = await SemearOrganizacaoAsync("Aurora Onboarding");
        var cliente = await ClienteAutenticado(conta.EmailDoDono);

        var resposta = await cliente.PatchAsJsonAsync("/organizacao", new { nome = "Imobiliária Aurora Ltda" });

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal("Imobiliária Aurora Ltda", await NoBanco(banco =>
            banco.Organizacoes.Where(o => o.Id == conta.OrganizacaoId).Select(o => o.Nome).SingleAsync()));
    }

    [Fact]
    public async Task Corretor_recebe_403_ao_tentar_renomear()
    {
        var conta = await SemearOrganizacaoAsync("Bela Vista Onboarding");
        var corretor = await SemearUsuarioAsync(
            conta.OrganizacaoId, PapeisDoUsuario.Corretor, "Corretor Bela Vista");
        var cliente = await ClienteAutenticado(corretor.Email);

        var resposta = await cliente.PatchAsJsonAsync("/organizacao", new { nome = "Outro Nome" });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Nome_vazio_e_recusado_sem_alterar_a_organizacao()
    {
        var conta = await SemearOrganizacaoAsync("Cristal Onboarding");
        var cliente = await ClienteAutenticado(conta.EmailDoDono);

        var resposta = await cliente.PatchAsJsonAsync("/organizacao", new { nome = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Cristal Onboarding", await NoBanco(banco =>
            banco.Organizacoes.Where(o => o.Id == conta.OrganizacaoId).Select(o => o.Nome).SingleAsync()));
    }

    [Fact]
    public async Task Qualquer_papel_atualiza_o_proprio_nome()
    {
        var conta = await SemearOrganizacaoAsync("Delta Onboarding");
        var corretor = await SemearUsuarioAsync(
            conta.OrganizacaoId, PapeisDoUsuario.Corretor, "Corretor Delta");
        var cliente = await ClienteAutenticado(corretor.Email);

        var resposta = await cliente.PatchAsJsonAsync(
            "/organizacao/usuarios/atual", new { nomeCompleto = "Corretor Delta Completo" });

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact]
    public async Task Sem_sessao_recebe_401_em_ambas_as_rotas()
    {
        var semSessao = Ambiente.Aplicacao.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await semSessao.PatchAsJsonAsync("/organizacao", new { nome = "Não importa" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await semSessao.PatchAsJsonAsync("/organizacao/usuarios/atual", new { nomeCompleto = "Não importa" }))
                .StatusCode);
    }

    private async Task<HttpClient> ClienteAutenticado(string email)
    {
        var cliente = Ambiente.Aplicacao.CreateClient();
        var entrada = await cliente.PostAsJsonAsync("/sessoes", new { email, senha = SenhaDeTeste });
        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);
        return cliente;
    }
}
