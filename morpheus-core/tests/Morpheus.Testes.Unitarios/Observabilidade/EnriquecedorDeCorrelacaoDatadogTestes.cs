using System.Diagnostics;
using System.Globalization;
using Morpheus.Api.Observabilidade;
using Morpheus.Testes.Unitarios.Fakes;
using Serilog;

namespace Morpheus.Testes.Unitarios.Observabilidade;

/// <summary>
/// Prova a correlação log↔trace para APM que lê ids decimais (Datadog): com uma
/// <see cref="Activity"/> ativa, o enricher converte o trace/span hexadecimal do
/// OpenTelemetry em decimal; sem atividade, não inventa nada.
/// </summary>
public sealed class EnriquecedorDeCorrelacaoDatadogTestes : IDisposable
{
    private static readonly ActivitySource Fonte = new("Morpheus.Testes");
    private readonly ActivityListener _ouvinte;

    public EnriquecedorDeCorrelacaoDatadogTestes()
    {
        _ouvinte = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(_ouvinte);
    }

    public void Dispose() => _ouvinte.Dispose();

    [Fact]
    public void Sem_atividade_nao_adiciona_ids_do_datadog()
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);

        diario.Information("linha sem trace");

        Assert.False(coletor.ContemPropriedade("dd.trace_id"));
        Assert.False(coletor.ContemPropriedade("dd.span_id"));
    }

    [Fact]
    public void Com_atividade_adiciona_trace_e_span_em_decimal()
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);

        using var atividade = Fonte.StartActivity("requisicao");
        diario.Information("linha com trace");

        Assert.NotNull(atividade);
        Assert.Equal(EsperadoDecimalDoTrace(atividade!), coletor.ValorDe("dd.trace_id"));
        Assert.Equal(EsperadoDecimalDoSpan(atividade!), coletor.ValorDe("dd.span_id"));
    }

    [Fact]
    public void Trace_id_do_datadog_usa_so_os_64_bits_baixos()
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);

        using var atividade = Fonte.StartActivity("requisicao");
        diario.Information("linha com trace");

        var decimalId = ulong.Parse(coletor.ValorDe("dd.trace_id")!, CultureInfo.InvariantCulture);
        Assert.Equal(EsperadoDecimalDoTrace(atividade!), decimalId.ToString(CultureInfo.InvariantCulture));
    }

    private static ILogger ConstruirDiario(ColetorDeEventosDeLog coletor) =>
        new LoggerConfiguration()
            .Enrich.With(new EnriquecedorDeCorrelacaoDatadog())
            .WriteTo.Sink(coletor)
            .CreateLogger();

    private static string EsperadoDecimalDoTrace(Activity atividade)
    {
        var hex = atividade.TraceId.ToHexString();
        return ulong.Parse(hex[^16..], NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            .ToString(CultureInfo.InvariantCulture);
    }

    private static string EsperadoDecimalDoSpan(Activity atividade) =>
        ulong.Parse(atividade.SpanId.ToHexString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            .ToString(CultureInfo.InvariantCulture);
}
