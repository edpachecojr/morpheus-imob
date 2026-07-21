namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// A matriz papel → permissões de [autorizacao.md](../../../../docs/fundacao/autorizacao.md),
/// expressa uma única vez em código. É a <b>fonte</b> da semeadura das claims de
/// papel no banco (tabela <c>role_claims</c>, semeada por migração): o banco é a
/// autoridade em tempo de execução, esta matriz é a autoridade em tempo de revisão —
/// mudança de permissão aparece no diff do PR (ADR-0010).
/// <para>
/// Só o que restringe é listado: o corretor recebe tudo menos as permissões de
/// gestão. Enumerar as negativas evitaria o esquecimento silencioso de liberar uma
/// permissão nova ao papel errado — o default de um papel novo é não ter nada.
/// </para>
/// </summary>
public static class MatrizDePermissoes
{
    // Gestão da conta: fora do alcance de quem apenas opera o dia a dia.
    private static readonly string[] NegadasAoCorretor =
    [
        PermissoesDoPainel.UsuarioGerenciar,
        PermissoesDoPainel.TenantConfigurar,
        PermissoesDoPainel.FaturamentoGerenciar,
        PermissoesDoPainel.MetricasLer,
    ];

    private static readonly Dictionary<string, IReadOnlyList<string>> PorPapel = new(StringComparer.Ordinal)
    {
        [PapeisDoUsuario.Dono] = PermissoesDoPainel.Todas,
        [PapeisDoUsuario.Corretor] = [.. PermissoesDoPainel.Todas.Except(NegadasAoCorretor)],
    };

    /// <summary>
    /// Permissões concedidas a um papel; lista vazia para papel desconhecido — o
    /// default nega, nunca libera. Exemplo:
    /// <c>MatrizDePermissoes.Do(PapeisDoUsuario.Corretor)</c>.
    /// </summary>
    public static IReadOnlyList<string> Do(string papel)
        => PorPapel.TryGetValue(papel, out var permissoes) ? permissoes : [];

    /// <summary>
    /// Se o papel concede a permissão. Exemplo:
    /// <c>MatrizDePermissoes.Concede(PapeisDoUsuario.Corretor, PermissoesDoPainel.UsuarioGerenciar)</c> → false.
    /// </summary>
    public static bool Concede(string papel, string permissao) => Do(papel).Contains(permissao);
}
