using Microsoft.Extensions.DependencyInjection;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Isolamento;

/// <summary>
/// Prova, contra um Postgres real, que a leitura no contexto de uma organização
/// nunca alcança dado de outra — pelos dois caminhos de leitura do sistema (EF
/// Core e Dapper) — e que a leitura sem contexto falha em vez de vazar.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class IsolamentoDeLeituraTestes : TesteDeIntegracao
{
    public IsolamentoDeLeituraTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Leitura_via_ef_core_retorna_somente_imoveis_da_organizacao_do_contexto()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        var belaVista = await SemearOrganizacaoAsync("Bela Vista");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-A1", "Rua Aurora, 1");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-A2", "Rua Aurora, 2");
        await SemearImovelAsync(belaVista.OrganizacaoId, "AP-B1", "Rua Bela Vista, 1");

        var imoveis = await ComoUsuario(aurora.UsuarioId, provedor =>
            provedor.GetRequiredService<IRepositorioDeImoveis>()
                    .ListarDaOrganizacaoAsync(CancellationToken.None));

        Assert.Equal(new[] { "AP-A1", "AP-A2" }, imoveis.Select(i => i.CodigoDeReferencia));
    }

    [Fact]
    public async Task Leitura_via_dapper_retorna_somente_imoveis_da_organizacao_do_contexto()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        var belaVista = await SemearOrganizacaoAsync("Bela Vista");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-A9", "Rua Aurora, 9");
        await SemearImovelAsync(belaVista.OrganizacaoId, "AP-B9", "Rua Bela Vista, 9");

        var resumos = await ComoUsuario(aurora.UsuarioId, ListarResumos);

        Assert.Equal(new[] { "AP-A9" }, resumos.Select(r => r.CodigoDeReferencia));
    }

    [Fact]
    public async Task Leitura_sem_contexto_de_usuario_falha_em_vez_de_retornar_dado()
    {
        await SemearOrganizacaoAsync("Aurora");

        await Assert.ThrowsAsync<ErroDeUsuarioNaoAutenticado>(() =>
            SemSessao(provedor =>
                provedor.GetRequiredService<IRepositorioDeImoveis>()
                        .ListarDaOrganizacaoAsync(CancellationToken.None)));
    }

    private static async Task<IReadOnlyList<ImovelResumo>> ListarResumos(IServiceProvider provedor)
        => await provedor.GetRequiredService<IConsultaDeImoveisResumidos>().ListarAsync(CancellationToken.None);
}
