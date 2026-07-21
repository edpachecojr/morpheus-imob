namespace Morpheus.Api.Endpoints;

/// <summary>
/// Um grupo coeso de rotas que sabe se mapear. Existe para que o
/// <c>Program.cs</c> não vire uma lista de <c>MapGet</c> que cresce sem fim: cada
/// implementação declara suas rotas, a permissão que exigem e nada mais.
/// <para>
/// A montagem é explícita em <see cref="MapeamentoDeEndpoints"/> — sem varredura
/// de assembly. Endpoint que ninguém listou não existe, e isso se lê num arquivo.
/// </para>
/// </summary>
public interface IEndpoint
{
    /// <summary>Registra as rotas deste grupo. Exemplo: <c>rotas.MapPost("/contas", Cadastrar)</c>.</summary>
    void Mapear(IEndpointRouteBuilder rotas);
}
