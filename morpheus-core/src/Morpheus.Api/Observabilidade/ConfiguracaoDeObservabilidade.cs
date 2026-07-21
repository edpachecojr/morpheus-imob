using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

namespace Morpheus.Api.Observabilidade;

/// <summary>
/// Compõe a observabilidade do host: Serilog (log estruturado lido do
/// appsettings), enrichers de correlação e sanitização, e OpenTelemetry para
/// rastreamento distribuído exportado por OTLP a qualquer APM. Mantém o
/// <c>Program.cs</c> magro e o restante do código cego ao SDK de observabilidade.
/// </summary>
public static class ConfiguracaoDeObservabilidade
{
    /// <summary>
    /// Logger mínimo para o intervalo entre o início do processo e a subida do
    /// host — captura falha de configuração que aconteceria antes do Serilog
    /// definitivo existir. Substituído por <see cref="AdicionarObservabilidade"/>.
    /// </summary>
    public static Serilog.ILogger CriarLogInicial() =>
        new LoggerConfiguration()
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateBootstrapLogger();

    public static WebApplicationBuilder AdicionarObservabilidade(this WebApplicationBuilder construtor)
    {
        construtor.Services.Configure<OpcoesDeRastreamento>(
            construtor.Configuration.GetSection(OpcoesDeRastreamento.Secao));

        RegistrarEnrichers(construtor.Services);
        AdicionarRastreamento(construtor);

        // ReadFrom.Services capta os enrichers registrados no DI; ReadFrom.Configuration
        // lê níveis, sinks e propriedades do appsettings — nada de sink em código.
        construtor.Services.AddSerilog((provedor, configuracao) => configuracao
            .ReadFrom.Configuration(construtor.Configuration)
            .ReadFrom.Services(provedor));

        return construtor;
    }

    public static WebApplication UsarObservabilidade(this WebApplication aplicacao)
    {
        aplicacao.UseMiddleware<EscopoDeLogDaOrganizacao>();
        aplicacao.UseSerilogRequestLogging(opcoes =>
            opcoes.EnrichDiagnosticContext = EnriquecerRequisicao);
        return aplicacao;
    }

    private static void RegistrarEnrichers(IServiceCollection servicos)
    {
        servicos.AddSingleton<ILogEventEnricher, EnriquecedorDeCorrelacaoDatadog>();
        servicos.AddSingleton<ILogEventEnricher, RedatorDeCamposSensiveis>();
    }

    private static void AdicionarRastreamento(WebApplicationBuilder construtor)
    {
        var opcoes = construtor.Configuration
            .GetSection(OpcoesDeRastreamento.Secao)
            .Get<OpcoesDeRastreamento>() ?? new OpcoesDeRastreamento();

        construtor.Services.AddOpenTelemetry()
            .ConfigureResource(recurso => recurso.AddService(
                serviceName: opcoes.NomeDoServico,
                serviceVersion: VersaoDoServico()))
            .WithTracing(rastreamento => ConfigurarRastreamento(rastreamento, opcoes));
    }

    private static void ConfigurarRastreamento(TracerProviderBuilder rastreamento, OpcoesDeRastreamento opcoes)
    {
        rastreamento
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(opcoes.EndpointOtlp))
            rastreamento.AddOtlpExporter(exportador => ConfigurarOtlp(exportador, opcoes));
    }

    private static void ConfigurarOtlp(OtlpExporterOptions exportador, OpcoesDeRastreamento opcoes)
    {
        exportador.Endpoint = new Uri(opcoes.EndpointOtlp!);
        exportador.Protocol = opcoes.ProtocoloOtlp.Equals("httpprotobuf", StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;
    }

    // Sem IP nem header do cliente: IP é dado pessoal e não entra no log (CLAUDE.md §Logging).
    private static void EnriquecerRequisicao(IDiagnosticContext contexto, HttpContext http)
    {
        contexto.Set("host_da_requisicao", http.Request.Host.Value);
        contexto.Set("esquema_da_requisicao", http.Request.Scheme);
        contexto.Set("protocolo_da_requisicao", http.Request.Protocol);
    }

    private static string VersaoDoServico() =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";
}
