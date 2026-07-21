using Serilog.Core;
using Serilog.Events;

namespace Morpheus.Api.Observabilidade;

/// <summary>
/// Mascara o valor de qualquer propriedade de log cujo nome sugira segredo ou
/// dado pessoal (senha, token, hash, cpf...). Defesa em profundidade para a regra
/// do CLAUDE.md e o critério E1-F4-H1: a regra primária é nunca passar segredo ao
/// log, mas um property bag enriquecido pode carregar um campo sensível sem
/// querer — aqui ele sai como <c>"***"</c>.
///
/// Cobre apenas propriedades estruturadas. Segredo interpolado direto no texto da
/// mensagem não é propriedade e não pode ser detectado — por isso os sinks usam
/// formatador JSON que emite o template com placeholders, não o texto renderizado.
///
/// Exemplo: um log com a propriedade <c>Senha = "abc123"</c> é emitido com
/// <c>"Senha":"***"</c>.
/// </summary>
public sealed class RedatorDeCamposSensiveis : ILogEventEnricher
{
    public const string Mascara = "***";

    private static readonly string[] TermosSensiveis =
    [
        "senha", "password", "token", "secret", "authorization",
        "apikey", "api_key", "hash", "credential", "cpf", "cnpj"
    ];

    public void Enrich(LogEvent evento, ILogEventPropertyFactory fabricaDePropriedade)
    {
        foreach (var nome in NomesSensiveis(evento))
            evento.AddOrUpdateProperty(new LogEventProperty(nome, new ScalarValue(Mascara)));
    }

    // Materializa antes de alterar: não dá para mexer no dicionário enquanto itera.
    private static IReadOnlyList<string> NomesSensiveis(LogEvent evento) =>
        evento.Properties.Keys.Where(EhSensivel).ToArray();

    private static bool EhSensivel(string nome) =>
        TermosSensiveis.Any(termo => nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
}
