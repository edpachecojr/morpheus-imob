using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Isolamento;

/// <summary>
/// Prova que a integridade referencial do vínculo de organização é imposta pelo
/// banco, não pela aplicação: um imóvel apontando para uma organização que não
/// existe é rejeitado pela FK. Agregados-filho (dossiê, anexo) herdarão a mesma
/// barreira contra FK que cruza organização quando entrarem no modelo.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class IntegridadeReferencialEntreOrganizacoesTestes : TesteDeIntegracao
{
    public IntegridadeReferencialEntreOrganizacoesTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Imovel_vinculado_a_organizacao_inexistente_e_rejeitado_pelo_banco()
    {
        var organizacaoInexistente = Guid.NewGuid();

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            SemSessao(async provedor =>
            {
                var banco = provedor.GetRequiredService<MorpheusDbContext>();
                var imovel = Imovel.Cadastrar(
                    new OrganizacaoDona(organizacaoInexistente), "AP-700", "Rua Fantasma, 700", TimeProvider.System).Valor;
                banco.Imoveis.Add(imovel);
                await banco.SaveChangesAsync();
            }));
    }
}
