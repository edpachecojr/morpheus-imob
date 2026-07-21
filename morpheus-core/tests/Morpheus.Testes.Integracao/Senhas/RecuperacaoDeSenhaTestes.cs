using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Senhas;

/// <summary>
/// Prova a recuperação e a redefinição de senha (E1-F2-H3) contra o provedor de
/// tokens real: resposta genérica exista ou não a conta, token de uso único e
/// queda de todas as sessões abertas quando a senha muda.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class RecuperacaoDeSenhaTestes : TesteDeIntegracao
{
    private const string NovaSenha = "outra-senha-bem-longa";

    public RecuperacaoDeSenhaTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Pedido_responde_igual_para_conta_existente_e_inexistente()
    {
        var conta = await SemearOrganizacaoAsync("Aurora Senha");

        var existente = await Pedir(conta.EmailDoDono);
        var inexistente = await Pedir($"ninguem-{Guid.NewGuid():N}@exemplo.test");

        Assert.Equal(HttpStatusCode.Accepted, existente.Status);
        Assert.Equal(existente, inexistente);
    }

    [Fact]
    public async Task Token_valido_troca_a_senha_e_o_login_passa_a_usar_a_nova()
    {
        var conta = await SemearOrganizacaoAsync("Bela Vista Senha");
        var token = await EmitirTokenAsync(conta.UsuarioId);

        var redefinicao = await Redefinir(conta.EmailDoDono, token, NovaSenha);

        Assert.Equal(HttpStatusCode.NoContent, redefinicao.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Entrar(conta.EmailDoDono, NovaSenha)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Entrar(conta.EmailDoDono, SenhaDeTeste)).StatusCode);
    }

    [Fact]
    public async Task Token_ja_usado_e_recusado_na_segunda_vez()
    {
        var conta = await SemearOrganizacaoAsync("Cristal Senha");
        var token = await EmitirTokenAsync(conta.UsuarioId);
        await Redefinir(conta.EmailDoDono, token, NovaSenha);

        var segunda = await Redefinir(conta.EmailDoDono, token, "mais-uma-senha-longa");

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task Token_forjado_e_recusado_com_a_mesma_resposta_de_token_expirado()
    {
        var conta = await SemearOrganizacaoAsync("Delta Senha");

        var forjado = await Redefinir(conta.EmailDoDono, "token-inventado", NovaSenha);
        var deContaInexistente = await Redefinir(
            $"ninguem-{Guid.NewGuid():N}@exemplo.test", "token-inventado", NovaSenha);

        Assert.Equal(HttpStatusCode.BadRequest, forjado.StatusCode);
        Assert.Equal(
            await forjado.Content.ReadAsStringAsync(),
            await deContaInexistente.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Troca_de_senha_derruba_as_sessoes_abertas_em_outros_aparelhos()
    {
        var conta = await SemearOrganizacaoAsync("Eco Senha");
        var aparelho = Ambiente.Aplicacao.CreateClient();
        await aparelho.PostAsJsonAsync("/sessoes", new { email = conta.EmailDoDono, senha = SenhaDeTeste });
        Assert.Equal(HttpStatusCode.OK, (await aparelho.GetAsync("/imoveis")).StatusCode);

        await Redefinir(conta.EmailDoDono, await EmitirTokenAsync(conta.UsuarioId), NovaSenha);

        Assert.Equal(HttpStatusCode.Unauthorized, (await aparelho.GetAsync("/imoveis")).StatusCode);
        Assert.Equal(0, await NoBanco(banco =>
            banco.Sessoes.CountAsync(sessao => sessao.UsuarioId == conta.UsuarioId)));
    }

    [Fact]
    public async Task Confirmacao_divergente_e_recusada_sem_consumir_o_token()
    {
        var conta = await SemearOrganizacaoAsync("Fenix Senha");
        var token = await EmitirTokenAsync(conta.UsuarioId);

        var divergente = await Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/senhas/redefinicoes", new
        {
            email = conta.EmailDoDono,
            token,
            novaSenha = NovaSenha,
            confirmacaoDeSenha = "nao-confere-mas-longa",
        });

        Assert.Equal(HttpStatusCode.BadRequest, divergente.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Redefinir(conta.EmailDoDono, token, NovaSenha)).StatusCode);
    }

    // O token viaja por e-mail em produção; aqui ele é emitido pelo mesmo provedor
    // que a rota de recuperação usa, sem depender de caixa de entrada.
    private async Task<string> EmitirTokenAsync(Guid usuarioId)
    {
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var registro = escopo.ServiceProvider.GetRequiredService<UserManager<UsuarioDaOrganizacao>>();
        var usuario = await registro.FindByIdAsync(usuarioId.ToString());
        return await registro.GeneratePasswordResetTokenAsync(usuario!);
    }

    private async Task<PedidoObservavel> Pedir(string email)
    {
        var resposta = await Ambiente.Aplicacao.CreateClient()
            .PostAsJsonAsync("/senhas/recuperacoes", new { email });
        return new PedidoObservavel(resposta.StatusCode, await resposta.Content.ReadAsStringAsync());
    }

    private Task<HttpResponseMessage> Redefinir(string email, string token, string novaSenha) =>
        Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/senhas/redefinicoes", new
        {
            email,
            token,
            novaSenha,
            confirmacaoDeSenha = novaSenha,
        });

    private Task<HttpResponseMessage> Entrar(string email, string senha) =>
        Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/sessoes", new { email, senha });

    /// <summary>Tudo que um cliente observa do pedido de recuperação.</summary>
    private sealed record PedidoObservavel(HttpStatusCode Status, string Corpo);
}
