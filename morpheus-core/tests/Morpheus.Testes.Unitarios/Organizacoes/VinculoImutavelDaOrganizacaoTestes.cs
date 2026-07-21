using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Identidade;

namespace Morpheus.Testes.Unitarios.Organizacoes;

/// <summary>
/// Prova a imutabilidade do vínculo de tenant onde ela ainda é aplicada em duas
/// fases: o usuário do Identity, que o SDK constrói sem parâmetros. As entidades de
/// domínio recebem o tenant na construção e não têm operação de revínculo.
/// </summary>
public sealed class VinculoImutavelDaOrganizacaoTestes
{
    private static readonly OrganizacaoDona Organizacao =
        new(Guid.Parse("77777777-7777-7777-7777-777777777777"));

    private static readonly OrganizacaoDona OutraOrganizacao =
        new(Guid.Parse("88888888-8888-8888-8888-888888888888"));

    [Fact]
    public void Vincular_a_mesma_organizacao_duas_vezes_e_idempotente()
    {
        var usuario = new UsuarioDaOrganizacao();

        usuario.VincularAOrganizacao(Organizacao);
        usuario.VincularAOrganizacao(Organizacao);

        Assert.Equal(Organizacao.Valor, usuario.OrganizacaoId);
    }

    [Fact]
    public void Revincular_para_outra_organizacao_falha()
    {
        var usuario = new UsuarioDaOrganizacao();
        usuario.VincularAOrganizacao(Organizacao);

        var erro = Assert.Throws<ErroDeVinculoDeOrganizacaoImutavel>(
            () => usuario.VincularAOrganizacao(OutraOrganizacao));

        Assert.Equal(Organizacao.Valor, erro.VinculoAtual);
        Assert.Equal(OutraOrganizacao.Valor, erro.VinculoRecusado);
    }
}
