using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Morpheus.Dominio.Imoveis;
using Morpheus.Infraestrutura.Persistencia;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Api;

/// <summary>
/// Prova, pela porta HTTP real, a busca, o filtro e a paginação de imóveis
/// (E2-F1-H2): isolamento por tenant, busca por código/título/endereço, filtro
/// por finalidade/situação e rejeição de paginação inválida com ProblemDetails.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class ListagemDeImoveisTestes : TesteDeIntegracao
{
    public ListagemDeImoveisTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Lista_somente_os_imoveis_da_organizacao_do_contexto()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora Listagem");
        var belaVista = await SemearOrganizacaoAsync("Bela Vista Listagem");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-L1", "Rua Aurora, 1");
        await SemearImovelAsync(belaVista.OrganizacaoId, "AP-L2", "Rua Bela Vista, 1");
        var cliente = await ClienteAutenticado(aurora.EmailDoDono);

        var resposta = await cliente.GetFromJsonAsync<RespostaDeListagem>("/imoveis");

        Assert.Equal("AP-L1", Assert.Single(resposta!.Itens).CodigoDeReferencia);
        Assert.Equal(1, resposta.Total);
    }

    [Theory]
    [InlineData("AP-B2")]
    [InlineData("Casa com quintal")]
    [InlineData("Rua das Begônias")]
    public async Task Busca_casa_por_codigo_titulo_ou_endereco(string termoDeBusca)
    {
        var organizacao = await SemearOrganizacaoAsync($"Busca {termoDeBusca}");
        await SemearImovelAsync(
            organizacao.OrganizacaoId, "AP-B2", "Casa com quintal", FinalidadeDoImovel.Venda, "Rua das Begônias, 20");
        await SemearImovelAsync(
            organizacao.OrganizacaoId, "AP-B3", "Cobertura duplex", FinalidadeDoImovel.Locacao, "Rua dos Lírios, 30");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetFromJsonAsync<RespostaDeListagem>($"/imoveis?busca={Uri.EscapeDataString(termoDeBusca)}");

        Assert.Equal("AP-B2", Assert.Single(resposta!.Itens).CodigoDeReferencia);
    }

    [Fact]
    public async Task Filtra_por_finalidade()
    {
        var organizacao = await SemearOrganizacaoAsync("Filtro Finalidade");
        await SemearImovelAsync(
            organizacao.OrganizacaoId, "AP-F1", "Título 1", FinalidadeDoImovel.Locacao, "Rua 1, 1");
        await SemearImovelAsync(
            organizacao.OrganizacaoId, "AP-F2", "Título 2", FinalidadeDoImovel.Venda, "Rua 2, 2");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetFromJsonAsync<RespostaDeListagem>("/imoveis?finalidade=Venda");

        Assert.Equal("AP-F2", Assert.Single(resposta!.Itens).CodigoDeReferencia);
    }

    [Fact]
    public async Task Filtra_por_situacao()
    {
        var organizacao = await SemearOrganizacaoAsync("Filtro Situacao");
        await SemearImovelAsync(organizacao.OrganizacaoId, "AP-S1", "Reservado", FinalidadeDoImovel.Venda, "Rua 1, 1");
        await SemearImovelAsync(organizacao.OrganizacaoId, "AP-S2", "Disponível", FinalidadeDoImovel.Venda, "Rua 2, 2");
        await MarcarComoReservadoAsync(organizacao.OrganizacaoId, "AP-S1");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetFromJsonAsync<RespostaDeListagem>("/imoveis?situacao=Reservado");

        Assert.Equal("AP-S1", Assert.Single(resposta!.Itens).CodigoDeReferencia);
    }

    [Fact]
    public async Task Pagina_com_tamanho_pedido_e_informa_o_total_da_organizacao()
    {
        var organizacao = await SemearOrganizacaoAsync("Paginacao");
        for (var indice = 1; indice <= 3; indice++)
            await SemearImovelAsync(organizacao.OrganizacaoId, $"AP-P{indice}", $"Rua P, {indice}");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetFromJsonAsync<RespostaDeListagem>("/imoveis?pagina=1&tamanhoDaPagina=2");

        Assert.Equal(2, resposta!.Itens.Count);
        Assert.Equal(3, resposta.Total);
        Assert.Equal(1, resposta.Pagina);
        Assert.Equal(2, resposta.TamanhoDaPagina);
    }

    [Fact]
    public async Task Finalidade_desconhecida_recebe_400_com_o_valor_recebido_na_mensagem()
    {
        var organizacao = await SemearOrganizacaoAsync("Finalidade Invalida");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetAsync("/imoveis?finalidade=alugueltemporada");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("alugueltemporada", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Pagina_menor_que_um_recebe_400()
    {
        var organizacao = await SemearOrganizacaoAsync("Pagina Invalida");
        var cliente = await ClienteAutenticado(organizacao.EmailDoDono);

        var resposta = await cliente.GetAsync("/imoveis?pagina=0");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private async Task MarcarComoReservadoAsync(Guid organizacaoId, string codigo)
    {
        // H3 (mudar situação) ainda não existe; o teste ajusta o dado direto no
        // banco só para provar que o FILTRO da leitura funciona.
        await NoBanco(async banco =>
        {
            await banco.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE imoveis SET situacao = 'Reservado' WHERE organizacao_id = {organizacaoId} AND codigo_de_referencia = {codigo}");
            return 0;
        });
    }

    private async Task<HttpClient> ClienteAutenticado(string email)
    {
        var cliente = Ambiente.Aplicacao.CreateClient();
        var entrada = await cliente.PostAsJsonAsync("/sessoes", new { email, senha = SenhaDeTeste });
        Assert.Equal(HttpStatusCode.NoContent, entrada.StatusCode);
        return cliente;
    }

    private sealed record RespostaDeListagem(IReadOnlyList<ItemDeListagem> Itens, int Total, int Pagina, int TamanhoDaPagina);

    private sealed record ItemDeListagem(Guid Id, string CodigoDeReferencia, string Titulo, string Finalidade, string Situacao, string Endereco);
}
