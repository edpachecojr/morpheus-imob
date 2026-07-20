using Microsoft.Extensions.DependencyInjection;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Isolamento;

/// <summary>
/// Prova, contra um Postgres real, as duas barreiras de escrita do interceptor:
/// gravar com vínculo divergente do contexto é rejeitado, e gravar sem contexto
/// e sem vínculo explícito (caminho job/bootstrap) também — nunca grava "global".
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class IsolamentoDeEscritaTestes : TesteDeIntegracao
{
    public IsolamentoDeEscritaTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Escrita_com_organizacao_divergente_do_contexto_e_rejeitada()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        var belaVista = await SemearOrganizacaoAsync("Bela Vista");

        var erro = await Assert.ThrowsAsync<ErroDeOrganizacaoDivergente>(() =>
            ComoUsuario(aurora.UsuarioId, async provedor =>
            {
                var banco = provedor.GetRequiredService<MorpheusDbContext>();
                var imovelDeOutraOrganizacao = new Imovel("AP-500", "Rua Bela Vista, 500", TimeProvider.System);
                imovelDeOutraOrganizacao.AtribuirOrganizacao(belaVista.OrganizacaoId);
                banco.Imoveis.Add(imovelDeOutraOrganizacao);
                await banco.SaveChangesAsync();
            }));

        Assert.Equal(belaVista.OrganizacaoId, erro.OrganizacaoDaEntidade);
        Assert.Equal(aurora.OrganizacaoId, erro.OrganizacaoDoContexto);
    }

    [Fact]
    public async Task Escrita_sem_contexto_e_sem_vinculo_explicito_e_rejeitada()
    {
        await Assert.ThrowsAsync<ErroDeEscritaSemOrganizacao>(() =>
            SemSessao(async provedor =>
            {
                var banco = provedor.GetRequiredService<MorpheusDbContext>();
                banco.Imoveis.Add(new Imovel("AP-600", "Rua Sem Dono, 600", TimeProvider.System));
                await banco.SaveChangesAsync();
            }));
    }
}
