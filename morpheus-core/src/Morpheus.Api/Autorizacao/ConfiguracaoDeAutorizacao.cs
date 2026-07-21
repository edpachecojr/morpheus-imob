using Microsoft.AspNetCore.Authorization;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Registra uma política por permissão nomeada do catálogo e o default que nega.
/// Registrar todas de antemão, em vez de resolver política sob demanda, torna
/// impossível uma rota exigir permissão inexistente: a política não existe e o
/// host falha na subida, não em produção.
/// </summary>
public static class ConfiguracaoDeAutorizacao
{
    private const string PrefixoDaPolitica = "permissao:";

    /// <summary>Nome da política de uma permissão. Exemplo: <c>permissao:imovel.ler</c>.</summary>
    public static string NomeDaPolitica(string permissao) => PrefixoDaPolitica + permissao;

    public static IServiceCollection AdicionarAutorizacaoPorPermissao(this IServiceCollection servicos)
    {
        servicos.AddSingleton<IAuthorizationHandler, VerificacaoDePermissaoDaRota>();

        servicos.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
                .AdicionarPoliticasDePermissao();

        return servicos;
    }

    // Fallback exige apenas sessão: rota sem declaração cai aqui e responde 401 em
    // vez de 200. A exigência de que TODA rota declare permissão ou anonimato é
    // provada na subida por ValidadorDeDeclaracaoDePermissao.
    private static AuthorizationBuilder AdicionarPoliticasDePermissao(this AuthorizationBuilder construtor)
    {
        foreach (var permissao in PermissoesDoPainel.Todas)
            construtor.AddPolicy(NomeDaPolitica(permissao), politica =>
                politica.RequireAuthenticatedUser().AddRequirements(new ExigenciaDePermissao(permissao)));

        return construtor;
    }
}
