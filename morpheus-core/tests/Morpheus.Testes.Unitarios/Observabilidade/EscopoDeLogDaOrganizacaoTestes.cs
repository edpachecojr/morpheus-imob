using Microsoft.AspNetCore.Http;
using Morpheus.Api.Observabilidade;
using Morpheus.Testes.Unitarios.Fakes;
using Serilog;

namespace Morpheus.Testes.Unitarios.Observabilidade;

/// <summary>
/// Prova que a organização do contexto vira <c>organizacao_id</c> em toda linha
/// de log da requisição (E1-F4-H1). Sem sessão, o campo não aparece — o default
/// é seguro, nunca uma organização inventada.
/// </summary>
public sealed class EscopoDeLogDaOrganizacaoTestes
{
    private static readonly Guid Organizacao = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public async Task Com_organizacao_no_contexto_o_log_carrega_organizacao_id()
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);
        var escopo = new EscopoDeLogDaOrganizacao(_ => { diario.Information("dentro da requisição"); return Task.CompletedTask; });

        await escopo.InvokeAsync(new DefaultHttpContext(), ContextoDaOrganizacaoAtualFake.Com(Organizacao));

        Assert.Equal(Organizacao.ToString(), coletor.ValorDe(EscopoDeLogDaOrganizacao.CampoOrganizacao));
    }

    [Fact]
    public async Task Sem_sessao_o_log_nao_carrega_organizacao_id()
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);
        var escopo = new EscopoDeLogDaOrganizacao(_ => { diario.Information("dentro da requisição"); return Task.CompletedTask; });

        await escopo.InvokeAsync(new DefaultHttpContext(), ContextoDaOrganizacaoAtualFake.SemSessao());

        Assert.False(coletor.ContemPropriedade(EscopoDeLogDaOrganizacao.CampoOrganizacao));
    }

    private static ILogger ConstruirDiario(ColetorDeEventosDeLog coletor) =>
        new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(coletor)
            .CreateLogger();
}
