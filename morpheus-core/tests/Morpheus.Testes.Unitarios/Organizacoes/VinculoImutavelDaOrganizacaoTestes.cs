using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

public sealed class VinculoImutavelDaOrganizacaoTestes
{
    private static readonly Guid Organizacao = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid OutraOrganizacao = Guid.Parse("88888888-8888-8888-8888-888888888888");

    private static Imovel NovoImovel() => new("AP-202", "Av. Central, 200", TimeProvider.System);

    [Fact]
    public void Atribuir_a_mesma_organizacao_duas_vezes_e_idempotente()
    {
        var imovel = NovoImovel();

        imovel.AtribuirOrganizacao(Organizacao);
        imovel.AtribuirOrganizacao(Organizacao);

        Assert.Equal(Organizacao, imovel.OrganizacaoId);
    }

    [Fact]
    public void Revincular_para_outra_organizacao_falha()
    {
        var imovel = NovoImovel();
        imovel.AtribuirOrganizacao(Organizacao);

        var erro = Assert.Throws<ErroDeVinculoDeOrganizacaoImutavel>(
            () => imovel.AtribuirOrganizacao(OutraOrganizacao));

        Assert.Equal(Organizacao, erro.VinculoAtual);
        Assert.Equal(OutraOrganizacao, erro.VinculoRecusado);
    }

    [Fact]
    public void Atribuir_organizacao_vazia_falha()
    {
        var imovel = NovoImovel();

        Assert.Throws<ArgumentException>(() => imovel.AtribuirOrganizacao(Guid.Empty));
    }
}
