namespace Morpheus.Api.Observabilidade;

/// <summary>
/// Configuração do rastreamento distribuído (OpenTelemetry), lida da seção
/// "Rastreamento" do appsettings via <c>IOptions</c>. Mantém o host agnóstico de
/// APM: trocar de Seq para Datadog, Elastic ou qualquer coletor OTLP é só mudar
/// endpoint e protocolo, sem tocar em código.
///
/// Exemplo (appsettings.Development.json): apontar <c>EndpointOtlp</c> para o Seq
/// local em <c>http://localhost:5341/ingest/otlp/v1/traces</c> com protocolo
/// <c>httpprotobuf</c>.
/// </summary>
public sealed class OpcoesDeRastreamento
{
    public const string Secao = "Rastreamento";

    /// <summary>Nome do serviço reportado ao APM (resource <c>service.name</c>).</summary>
    public string NomeDoServico { get; init; } = "morpheus-api";

    /// <summary>
    /// Endpoint OTLP do coletor. Vazio desliga a exportação — é o padrão em
    /// ambiente sem coletor (testes, dev sem Seq), onde os traces ainda existem
    /// em memória para correlacionar o log, mas nada é enviado para fora.
    /// </summary>
    public string? EndpointOtlp { get; init; }

    /// <summary>Protocolo OTLP: <c>grpc</c> (padrão) ou <c>httpprotobuf</c>.</summary>
    public string ProtocoloOtlp { get; init; } = "grpc";
}
