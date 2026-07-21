using Morpheus.Dominio.Imoveis;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Imoveis;

public sealed class ImovelTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Organizacao = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void Cadastrar_gera_identidade_e_auditoria_do_relogio()
    {
        var resultado = Imovel.Cadastrar("AP-101", "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.True(resultado.Sucesso);
        Assert.NotEqual(Guid.Empty, resultado.Valor.Id);
        Assert.Equal(Instante, resultado.Valor.CadastradoEm);
    }

    [Fact]
    public void Cadastrar_apara_espacos_do_codigo_e_do_endereco()
    {
        var resultado = Imovel.Cadastrar("  AP-101  ", "  Rua das Acácias, 100  ", new RelogioFixo(Instante));

        Assert.Equal("AP-101", resultado.Valor.CodigoDeReferencia);
        Assert.Equal("Rua das Acácias, 100", resultado.Valor.Endereco);
    }

    [Fact]
    public void Cadastrar_sem_codigo_falha_com_codigo_obrigatorio()
    {
        var resultado = Imovel.Cadastrar("   ", "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeImovel.CodigoObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Cadastrar_sem_endereco_falha_com_endereco_obrigatorio()
    {
        var resultado = Imovel.Cadastrar("AP-101", "", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeImovel.EnderecoObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Rehidratar_preserva_os_atributos_sem_gerar_nova_identidade()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var imovel = Imovel.Rehidratar(id, Organizacao, "AP-101", "Rua das Acácias, 100", Instante, Instante);

        Assert.Equal(id, imovel.Id);
        Assert.Equal(Organizacao, imovel.OrganizacaoId);
        Assert.Equal("AP-101", imovel.CodigoDeReferencia);
        Assert.Equal(Instante, imovel.CadastradoEm);
    }

    [Fact]
    public void Rehidratar_nao_registra_evento_de_dominio()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var imovel = Imovel.Rehidratar(id, Organizacao, "AP-101", "Rua das Acácias, 100", Instante, Instante);

        Assert.Empty(imovel.EventosDeDominio);
    }
}
