namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Catálogo das permissões nomeadas do painel, conforme a matriz de
/// [autorizacao.md](../../../../docs/fundacao/autorizacao.md). Verificação é sempre
/// por permissão — comparar papel no código de negócio é proibido (ADR-0005),
/// porque quando entrar `gestor` alguém esquece um lugar.
/// <para>
/// Nome no formato <c>recurso.acao</c>: legível na claim, no log de negação e no
/// teste, sem tabela de-para.
/// </para>
/// </summary>
public static class PermissoesDoPainel
{
    public const string ImovelLer = "imovel.ler";
    public const string ImovelEscrever = "imovel.escrever";
    public const string LeadLer = "lead.ler";
    public const string LeadAtender = "lead.atender";
    public const string VisitaAgendar = "visita.agendar";
    public const string DossieCriar = "dossie.criar";
    public const string DossieLer = "dossie.ler";
    public const string DossieAprovarItem = "dossie.aprovar_item";
    public const string DossieBaixarAnexo = "dossie.baixar_anexo";
    public const string MagicLinkEmitir = "magiclink.emitir";
    public const string MagicLinkRevogar = "magiclink.revogar";
    public const string RelatorioGerar = "relatorio.gerar";
    public const string RelatorioEnviar = "relatorio.enviar";
    public const string UsuarioGerenciar = "usuario.gerenciar";
    public const string TenantConfigurar = "tenant.configurar";
    public const string FaturamentoGerenciar = "faturamento.gerenciar";
    public const string MetricasLer = "metricas.ler";

    /// <summary>
    /// Todas as permissões conhecidas, em ordem estável. É a lista sobre a qual o
    /// host registra uma política de autorização por permissão — endpoint que exija
    /// permissão fora daqui não encontra política e falha na subida, não em produção.
    /// </summary>
    public static readonly IReadOnlyList<string> Todas =
    [
        ImovelLer,
        ImovelEscrever,
        LeadLer,
        LeadAtender,
        VisitaAgendar,
        DossieCriar,
        DossieLer,
        DossieAprovarItem,
        DossieBaixarAnexo,
        MagicLinkEmitir,
        MagicLinkRevogar,
        RelatorioGerar,
        RelatorioEnviar,
        UsuarioGerenciar,
        TenantConfigurar,
        FaturamentoGerenciar,
        MetricasLer,
    ];
}
