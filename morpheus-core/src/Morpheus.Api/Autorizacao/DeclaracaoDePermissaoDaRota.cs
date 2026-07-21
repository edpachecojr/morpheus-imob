namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Como uma rota declara o acesso que exige. Único jeito de proteger rota no
/// projeto: junta, num gesto só, a política de autorização e o metadado que a
/// verificação de subida procura.
/// </summary>
public static class DeclaracaoDePermissaoDaRota
{
    /// <summary>
    /// Exige a permissão nomeada. Exemplo:
    /// <c>rotas.MapGet("/imoveis", Listar).RequerPermissao(PermissoesDoPainel.ImovelLer)</c>.
    /// </summary>
    public static RouteHandlerBuilder RequerPermissao(this RouteHandlerBuilder rota, string permissao) =>
        rota.RequireAuthorization(ConfiguracaoDeAutorizacao.NomeDaPolitica(permissao))
            .WithMetadata(new PermissaoExigida(permissao));

    /// <summary>
    /// Exige apenas sessão válida, sem permissão nomeada. Exemplo:
    /// <c>rotas.MapDelete("/sessoes/atual", Sair).RequerApenasSessao()</c>.
    /// </summary>
    public static RouteHandlerBuilder RequerApenasSessao(this RouteHandlerBuilder rota) =>
        rota.RequireAuthorization()
            .WithMetadata(new ApenasSessaoExigida());
}
