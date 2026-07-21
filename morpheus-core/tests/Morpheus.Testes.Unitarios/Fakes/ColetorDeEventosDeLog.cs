using Serilog.Core;
using Serilog.Events;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Sink Serilog em memória: guarda cada <see cref="LogEvent"/> emitido para o
/// teste inspecionar propriedades sem tocar em console, arquivo ou rede (F.I.R.S.T.).
/// </summary>
public sealed class ColetorDeEventosDeLog : ILogEventSink
{
    private readonly List<LogEvent> _eventos = [];

    public IReadOnlyList<LogEvent> Eventos => _eventos;

    public LogEvent UltimoEvento => _eventos[^1];

    public void Emit(LogEvent logEvent) => _eventos.Add(logEvent);

    public string? ValorDe(string propriedade) =>
        UltimoEvento.Properties.TryGetValue(propriedade, out var valor)
            ? (valor as ScalarValue)?.Value?.ToString()
            : null;

    public bool ContemPropriedade(string propriedade) =>
        UltimoEvento.Properties.ContainsKey(propriedade);
}
