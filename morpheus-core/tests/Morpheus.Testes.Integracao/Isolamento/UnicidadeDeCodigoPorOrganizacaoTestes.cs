using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Isolamento;

/// <summary>
/// Prova a unicidade composta (organizacao_id, codigo_de_referencia): o mesmo
/// código coexiste em organizações distintas, mas duplicá-lo dentro da mesma
/// organização é rejeitado pelo banco (E1-F1-H3).
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class UnicidadeDeCodigoPorOrganizacaoTestes : TesteDeIntegracao
{
    public UnicidadeDeCodigoPorOrganizacaoTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Mesmo_codigo_coexiste_em_organizacoes_distintas()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        var belaVista = await SemearOrganizacaoAsync("Bela Vista");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-101", "Rua Aurora, 101");
        await SemearImovelAsync(belaVista.OrganizacaoId, "AP-101", "Rua Bela Vista, 101");

        var deAurora = await ComoUsuario(aurora.UsuarioId, ListarResumos);
        var deBelaVista = await ComoUsuario(belaVista.UsuarioId, ListarResumos);

        Assert.Equal("AP-101", Assert.Single(deAurora).CodigoDeReferencia);
        Assert.Equal("AP-101", Assert.Single(deBelaVista).CodigoDeReferencia);
    }

    [Fact]
    public async Task Codigo_duplicado_na_mesma_organizacao_e_rejeitado()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-101", "Rua Aurora, 101");

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            SemearImovelAsync(aurora.OrganizacaoId, "AP-101", "Rua Aurora, 202"));
    }

    private static async Task<IReadOnlyList<ImovelResumo>> ListarResumos(IServiceProvider provedor)
        => await provedor.GetRequiredService<IConsultaDeImoveisResumidos>().ListarAsync(CancellationToken.None);
}
