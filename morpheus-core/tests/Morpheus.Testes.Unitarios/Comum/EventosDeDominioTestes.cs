using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Comum;

/// <summary>
/// Prova que as ações de escrita registram eventos de domínio com os dados de
/// negócio completos — a base do que o outbox depois grava.
/// </summary>
public sealed class EventosDeDominioTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Cadastrar_imovel_registra_evento_com_dados_completos()
    {
        var imovel = Imovel.Cadastrar("AP-101", "Rua das Acácias, 100", new RelogioFixo(Instante)).Valor;

        var evento = Assert.Single(imovel.EventosDeDominio);
        var cadastrado = Assert.IsType<ImovelCadastradoEvento>(evento);
        Assert.Equal(imovel.Id, cadastrado.ImovelId);
        Assert.Equal("AP-101", cadastrado.CodigoDeReferencia);
        Assert.Equal("Rua das Acácias, 100", cadastrado.Endereco);
        Assert.Equal(Instante, cadastrado.OcorridoEm);
    }

    [Fact]
    public void Fundar_organizacao_registra_evento_com_dados_completos()
    {
        var organizacao = Organizacao.Fundar("Imobiliária Aurora", new RelogioFixo(Instante)).Valor;

        var evento = Assert.Single(organizacao.EventosDeDominio);
        var fundada = Assert.IsType<OrganizacaoFundadaEvento>(evento);
        Assert.Equal(organizacao.Id, fundada.OrganizacaoId);
        Assert.Equal("Imobiliária Aurora", fundada.Nome);
        Assert.Equal(Instante, fundada.OcorridoEm);
    }

    [Fact]
    public void Cadastro_invalido_nao_registra_evento()
    {
        var resultado = Imovel.Cadastrar("   ", "Rua das Acácias, 100", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
    }

    [Fact]
    public void LimparEventos_esvazia_a_fila_de_eventos()
    {
        var imovel = Imovel.Cadastrar("AP-101", "Rua das Acácias, 100", new RelogioFixo(Instante)).Valor;

        imovel.LimparEventos();

        Assert.Empty(imovel.EventosDeDominio);
    }
}
