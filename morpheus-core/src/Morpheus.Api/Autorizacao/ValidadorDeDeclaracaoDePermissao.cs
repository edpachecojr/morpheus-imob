using Microsoft.AspNetCore.Authorization;

namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Prova, na subida, que toda rota declarou o que exige: ou uma
/// <see cref="PermissaoExigida"/>, ou anonimato explícito. Rota que esquecer as
/// duas coisas derruba o processo com o nome dela na mensagem — a falha aparece no
/// primeiro <c>dotnet run</c> ou na CI, não num pentest.
/// <para>
/// Lógica pura sobre metadados, sem host: é o que a torna testável por unidade.
/// </para>
/// </summary>
public static class ValidadorDeDeclaracaoDePermissao
{
    /// <summary>
    /// Nomes das rotas sem declaração, na ordem em que aparecem. Vazio quando está
    /// tudo declarado. Exemplo: <c>ValidadorDeDeclaracaoDePermissao.RotasSemDeclaracao(endpoints)</c>.
    /// </summary>
    public static IReadOnlyList<string> RotasSemDeclaracao(IEnumerable<Endpoint> endpoints) =>
        [.. endpoints.Where(SemDeclaracao).Select(endpoint => endpoint.DisplayName ?? "(rota sem nome)")];

    /// <summary>Falha alto quando alguma rota não declarou permissão nem anonimato.</summary>
    public static void GarantirQueTodasDeclaram(IEnumerable<Endpoint> endpoints)
    {
        var pendentes = RotasSemDeclaracao(endpoints);
        if (pendentes.Count == 0)
            return;

        throw new InvalidOperationException(
            $"Rotas sem declaração de acesso: {string.Join(", ", pendentes)}. " +
            "Toda rota deve chamar RequerPermissao(<permissao>), RequerApenasSessao() ou AllowAnonymous().");
    }

    private static bool SemDeclaracao(Endpoint endpoint)
        => endpoint.Metadata.GetMetadata<IDeclaracaoDeAcesso>() is null
            && endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null;
}
