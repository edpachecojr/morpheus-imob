using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

public sealed class RegraDeVinculoComOrganizacaoTestes
{
    private static readonly Guid OrganizacaoDoContexto = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid OutraOrganizacao = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static Imovel NovoImovel() => new("AP-101", "Rua das Acácias, 100", TimeProvider.System);

    [Fact]
    public void Carimba_o_vinculo_ausente_com_a_organizacao_do_contexto()
    {
        var imovel = NovoImovel();

        RegraDeVinculoComOrganizacao.GarantirVinculo(imovel, OrganizacaoDoContexto);

        Assert.Equal(OrganizacaoDoContexto, imovel.OrganizacaoId);
    }

    [Fact]
    public void Aceita_vinculo_igual_ao_contexto()
    {
        var imovel = NovoImovel();
        imovel.AtribuirOrganizacao(OrganizacaoDoContexto);

        RegraDeVinculoComOrganizacao.GarantirVinculo(imovel, OrganizacaoDoContexto);

        Assert.Equal(OrganizacaoDoContexto, imovel.OrganizacaoId);
    }

    [Fact]
    public void Rejeita_vinculo_divergente_citando_as_duas_organizacoes()
    {
        var imovel = NovoImovel();
        imovel.AtribuirOrganizacao(OutraOrganizacao);

        var erro = Assert.Throws<ErroDeOrganizacaoDivergente>(
            () => RegraDeVinculoComOrganizacao.GarantirVinculo(imovel, OrganizacaoDoContexto));

        Assert.Equal(OutraOrganizacao, erro.OrganizacaoDaEntidade);
        Assert.Equal(OrganizacaoDoContexto, erro.OrganizacaoDoContexto);
    }

    [Fact]
    public void Rejeita_contexto_de_organizacao_vazio()
    {
        var imovel = NovoImovel();

        Assert.Throws<ArgumentException>(
            () => RegraDeVinculoComOrganizacao.GarantirVinculo(imovel, Guid.Empty));
    }

    [Fact]
    public void Sem_contexto_exige_vinculo_explicito_na_entidade()
    {
        var imovel = NovoImovel();

        Assert.Throws<ErroDeEscritaSemOrganizacao>(
            () => RegraDeVinculoComOrganizacao.GarantirVinculoComContextoOpcional(imovel, null));
    }

    [Fact]
    public void Sem_contexto_aceita_entidade_com_vinculo_explicito()
    {
        var imovel = NovoImovel();
        imovel.AtribuirOrganizacao(OutraOrganizacao);

        RegraDeVinculoComOrganizacao.GarantirVinculoComContextoOpcional(imovel, null);

        Assert.Equal(OutraOrganizacao, imovel.OrganizacaoId);
    }
}
