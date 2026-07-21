namespace Morpheus.Api.Endpoints.Saude;

/// <summary>
/// Sonda de vida da aplicação (E1-F0-H2). Anônima por necessidade: quem consulta é
/// orquestrador de contêiner e balanceador, que não têm sessão.
/// </summary>
public sealed class EndpointDeSaude : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas) =>
        rotas.MapGet("/health", () => Results.Ok(new { status = "ok" }))
             .AllowAnonymous()
             .WithName("SaudeDaAplicacao");
}
