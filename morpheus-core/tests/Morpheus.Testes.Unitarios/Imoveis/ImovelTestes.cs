using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Imoveis;

public sealed class ImovelTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Organizacao = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly OrganizacaoDona Tenant = new(Organizacao);
    private const string Titulo = "Apartamento com vista para o parque";

    [Fact]
    public void Cadastrar_gera_identidade_e_auditoria_do_relogio()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "AP-101", Titulo, FinalidadeDoImovel.Locacao, "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.True(resultado.Sucesso);
        Assert.NotEqual(Guid.Empty, resultado.Valor.Id);
        Assert.Equal(Instante, resultado.Valor.CadastradoEm);
    }

    [Fact]
    public void Cadastrar_vincula_o_imovel_a_organizacao_recebida()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "AP-101", Titulo, FinalidadeDoImovel.Locacao, "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.Equal(Organizacao, resultado.Valor.OrganizacaoId);
    }

    [Fact]
    public void Cadastrar_nasce_sempre_disponivel()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "AP-101", Titulo, FinalidadeDoImovel.Venda, "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.Equal(SituacaoDoImovel.Disponivel, resultado.Valor.Situacao);
        Assert.Equal(FinalidadeDoImovel.Venda, resultado.Valor.Finalidade);
    }

    [Fact]
    public void Cadastrar_apara_espacos_do_codigo_do_titulo_e_do_endereco()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "  AP-101  ", "  " + Titulo + "  ", FinalidadeDoImovel.Locacao,
            "  Rua das Acácias, 100  ", new RelogioFixo(Instante));

        Assert.Equal("AP-101", resultado.Valor.CodigoDeReferencia);
        Assert.Equal(Titulo, resultado.Valor.Titulo);
        Assert.Equal("Rua das Acácias, 100", resultado.Valor.Endereco);
    }

    [Fact]
    public void Cadastrar_sem_codigo_falha_com_codigo_obrigatorio()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "   ", Titulo, FinalidadeDoImovel.Locacao, "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeImovel.CodigoObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Cadastrar_sem_titulo_falha_com_titulo_obrigatorio()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "AP-101", "   ", FinalidadeDoImovel.Locacao, "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeImovel.TituloObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Cadastrar_sem_endereco_falha_com_endereco_obrigatorio()
    {
        var resultado = Imovel.Cadastrar(
            Tenant, "AP-101", Titulo, FinalidadeDoImovel.Locacao, "", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeImovel.EnderecoObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Rehidratar_preserva_os_atributos_sem_gerar_nova_identidade()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var imovel = Imovel.Rehidratar(
            id, Organizacao, "AP-101", Titulo, FinalidadeDoImovel.Venda, SituacaoDoImovel.Reservado,
            "Rua das Acácias, 100", Instante, Instante);

        Assert.Equal(id, imovel.Id);
        Assert.Equal(Organizacao, imovel.OrganizacaoId);
        Assert.Equal("AP-101", imovel.CodigoDeReferencia);
        Assert.Equal(Titulo, imovel.Titulo);
        Assert.Equal(FinalidadeDoImovel.Venda, imovel.Finalidade);
        Assert.Equal(SituacaoDoImovel.Reservado, imovel.Situacao);
        Assert.Equal(Instante, imovel.CadastradoEm);
    }

    [Fact]
    public void Rehidratar_nao_registra_evento_de_dominio()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var imovel = Imovel.Rehidratar(
            id, Organizacao, "AP-101", Titulo, FinalidadeDoImovel.Locacao, SituacaoDoImovel.Disponivel,
            "Rua das Acácias, 100", Instante, Instante);

        Assert.Empty(imovel.EventosDeDominio);
    }
}
