using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

/// <summary>
/// Prova a invariante mínima do value object de tenant: guarda um id válido e
/// recusa o vazio na fronteira, para que nenhuma entidade nasça com tenant vazio.
/// </summary>
public sealed class OrganizacaoDonaTestes
{
    [Fact]
    public void Guarda_o_id_recebido()
    {
        var id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var organizacao = new OrganizacaoDona(id);

        Assert.Equal(id, organizacao.Valor);
    }

    [Fact]
    public void Recusa_id_vazio_citando_a_operacao()
    {
        var erro = Assert.Throws<ErroDeOrganizacaoObrigatoria>(() => new OrganizacaoDona(Guid.Empty));

        Assert.Equal(nameof(OrganizacaoDona), erro.Operacao);
    }

    [Fact]
    public void Dois_ids_com_o_mesmo_valor_sao_iguais()
    {
        var id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        Assert.Equal(new OrganizacaoDona(id), new OrganizacaoDona(id));
    }
}
