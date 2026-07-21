using Morpheus.Dominio.Resultados;

namespace Morpheus.Api.Endpoints;

/// <summary>
/// Traduz um <see cref="Erro"/> do domínio em ProblemDetails, com o código
/// estável como extensão para o cliente programar em cima da descrição legível.
/// Um lugar só, para que dois endpoints não inventem formatos de erro diferentes.
/// </summary>
internal static class RespostaDeFalha
{
    public const string CampoDoCodigo = "codigo";

    public static IResult De(Erro erro, int status) => Results.Problem(
        title: erro.Descricao,
        statusCode: status,
        extensions: new Dictionary<string, object?> { [CampoDoCodigo] = erro.Codigo });
}
