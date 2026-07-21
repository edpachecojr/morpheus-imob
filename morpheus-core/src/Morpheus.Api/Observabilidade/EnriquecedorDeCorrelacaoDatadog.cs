using System.Diagnostics;
using System.Globalization;
using Serilog.Core;
using Serilog.Events;

namespace Morpheus.Api.Observabilidade;

/// <summary>
/// Traduz o trace/span da <see cref="Activity"/> atual para os ids decimais que
/// o Datadog espera (<c>dd.trace_id</c>, <c>dd.span_id</c>), complementando os ids
/// hexadecimais W3C que o <c>Serilog.Enrichers.Span</c> já injeta. Sem essa
/// conversão o Datadog não correlaciona log e trace: o padrão OpenTelemetry emite
/// o id em hexadecimal e o Datadog lê em decimal.
///
/// Exemplo: com uma requisição em curso, um log emitido ganha
/// <c>"dd.trace_id":"1311768467294899695"</c>, derivado dos 64 bits baixos do
/// trace id de 128 bits.
/// </summary>
public sealed class EnriquecedorDeCorrelacaoDatadog : ILogEventEnricher
{
    private const string CampoTraceId = "dd.trace_id";
    private const string CampoSpanId = "dd.span_id";

    public void Enrich(LogEvent evento, ILogEventPropertyFactory fabricaDePropriedade)
    {
        var atividade = Activity.Current;
        if (atividade is null)
            return;

        AdicionarSeConvertivel(evento, fabricaDePropriedade, CampoTraceId, BitsBaixosDoTrace(atividade.TraceId));
        AdicionarSeConvertivel(evento, fabricaDePropriedade, CampoSpanId, atividade.SpanId.ToHexString());
    }

    // O Datadog usa só os 64 bits baixos do trace id de 128 bits — os últimos 16 hex.
    private static string BitsBaixosDoTrace(ActivityTraceId traceId)
    {
        var hexadecimal = traceId.ToHexString();
        return hexadecimal.Length >= 16 ? hexadecimal[^16..] : hexadecimal;
    }

    private static void AdicionarSeConvertivel(
        LogEvent evento, ILogEventPropertyFactory fabrica, string campo, string hexadecimal)
    {
        if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var decimalId))
            return;
        evento.AddPropertyIfAbsent(fabrica.CreateProperty(campo, decimalId.ToString(CultureInfo.InvariantCulture)));
    }
}
