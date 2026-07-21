using Microsoft.Extensions.Logging;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Logger <c>Microsoft.Extensions.Logging</c> falso que guarda cada entrada
/// (nível, mensagem renderizada e exceção) para o teste verificar o que foi
/// registrado, sem escrever em lugar nenhum.
/// </summary>
public sealed class DiarioDeOperacoesFake : ILogger
{
    public sealed record Entrada(LogLevel Nivel, string Mensagem, Exception? Excecao);

    private readonly List<Entrada> _entradas = [];

    public IReadOnlyList<Entrada> Entradas => _entradas;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        _entradas.Add(new Entrada(logLevel, formatter(state, exception), exception));
}

/// <summary>Variante tipada para satisfazer <c>ILogger&lt;T&gt;</c> no construtor de decoradores.</summary>
public sealed class DiarioDeOperacoesFake<T> : ILogger<T>
{
    private readonly DiarioDeOperacoesFake _interno = new();

    public IReadOnlyList<DiarioDeOperacoesFake.Entrada> Entradas => _interno.Entradas;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _interno.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _interno.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        _interno.Log(logLevel, eventId, state, exception, formatter);
}
