using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Usuarios;

/// <summary>
/// Confere a matriz de permissões contra a tabela documentada em
/// fundacao/autorizacao.md (E1-F3-H2). Cada permissão restrita tem seu teste
/// negativo — é onde a maioria dos sistemas falha, porque só se testa o caminho
/// feliz.
/// </summary>
public sealed class MatrizDePermissoesTestes
{
    [Fact]
    public void Dono_recebe_todas_as_permissoes_do_catalogo()
        => Assert.Equal(PermissoesDoPainel.Todas, MatrizDePermissoes.Do(PapeisDoUsuario.Dono));

    [Theory]
    [InlineData(PermissoesDoPainel.UsuarioGerenciar)]
    [InlineData(PermissoesDoPainel.FaturamentoGerenciar)]
    [InlineData(PermissoesDoPainel.TenantConfigurar)]
    [InlineData(PermissoesDoPainel.MetricasLer)]
    public void Corretor_nao_recebe_permissao_de_gestao(string permissao)
        => Assert.False(MatrizDePermissoes.Concede(PapeisDoUsuario.Corretor, permissao));

    [Theory]
    [InlineData(PermissoesDoPainel.ImovelLer)]
    [InlineData(PermissoesDoPainel.ImovelEscrever)]
    [InlineData(PermissoesDoPainel.LeadAtender)]
    [InlineData(PermissoesDoPainel.VisitaAgendar)]
    [InlineData(PermissoesDoPainel.DossieCriar)]
    [InlineData(PermissoesDoPainel.DossieBaixarAnexo)]
    [InlineData(PermissoesDoPainel.MagicLinkEmitir)]
    [InlineData(PermissoesDoPainel.RelatorioEnviar)]
    public void Corretor_recebe_as_permissoes_de_operacao(string permissao)
        => Assert.True(MatrizDePermissoes.Concede(PapeisDoUsuario.Corretor, permissao));

    [Fact]
    public void Papel_desconhecido_nao_recebe_nada()
        => Assert.Empty(MatrizDePermissoes.Do("gestor"));

    [Fact]
    public void Catalogo_nao_tem_permissao_repetida()
        => Assert.Equal(PermissoesDoPainel.Todas.Count, PermissoesDoPainel.Todas.Distinct().Count());
}
