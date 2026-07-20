using Morpheus.Dominio.Imoveis;
using Morpheus.Infraestrutura.Organizacoes;

namespace Morpheus.Testes.Unitarios.Imoveis;

public sealed class FiltroDaOrganizacaoTestes
{
    private static readonly Guid OrganizacaoA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OrganizacaoB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void Retorna_somente_os_imoveis_da_organizacao_pedida()
    {
        var deA = ImovelVinculado("AP-1", OrganizacaoA);
        var deB = ImovelVinculado("AP-2", OrganizacaoB);
        var consulta = new[] { deA, deB }.AsQueryable();

        var resultado = consulta.DaOrganizacao(OrganizacaoA).ToList();

        Assert.Single(resultado);
        Assert.Equal(deA.Id, resultado[0].Id);
    }

    [Fact]
    public void Nao_retorna_imovel_de_outra_organizacao()
    {
        var deB = ImovelVinculado("AP-3", OrganizacaoB);
        var consulta = new[] { deB }.AsQueryable();

        var resultado = consulta.DaOrganizacao(OrganizacaoA).ToList();

        Assert.Empty(resultado);
    }

    [Fact]
    public void Rejeita_organizacao_vazia()
    {
        var consulta = Array.Empty<Imovel>().AsQueryable();

        Assert.Throws<ArgumentException>(() => consulta.DaOrganizacao(Guid.Empty).ToList());
    }

    private static Imovel ImovelVinculado(string codigo, Guid organizacaoId)
    {
        var imovel = new Imovel(codigo, "Endereço qualquer, 1", TimeProvider.System);
        imovel.AtribuirOrganizacao(organizacaoId);
        return imovel;
    }
}
