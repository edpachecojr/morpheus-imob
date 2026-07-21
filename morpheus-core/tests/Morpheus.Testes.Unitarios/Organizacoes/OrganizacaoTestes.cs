using Morpheus.Dominio.Organizacoes;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

// A cobertura do evento OrganizacaoFundadaEvento vive em EventosDeDominioTestes.

public sealed class OrganizacaoTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Fundar_gera_identidade_e_auditoria_do_relogio()
    {
        var resultado = Organizacao.Fundar("Imobiliária Aurora", new RelogioFixo(Instante));

        Assert.True(resultado.Sucesso);
        Assert.NotEqual(Guid.Empty, resultado.Valor.Id);
        Assert.Equal(Instante, resultado.Valor.CriadaEm);
    }

    [Fact]
    public void Fundar_apara_espacos_do_nome()
    {
        var resultado = Organizacao.Fundar("  Imobiliária Aurora  ", new RelogioFixo(Instante));

        Assert.Equal("Imobiliária Aurora", resultado.Valor.Nome);
    }

    [Fact]
    public void Fundar_sem_nome_falha_com_nome_obrigatorio()
    {
        var resultado = Organizacao.Fundar("   ", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeOrganizacao.NomeObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Rehidratar_preserva_os_atributos_sem_gerar_nova_identidade()
    {
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var organizacao = Organizacao.Rehidratar(id, "Imobiliária Aurora", ConfiguracaoDaOrganizacao.Padrao(), Instante, Instante);

        Assert.Equal(id, organizacao.Id);
        Assert.Equal("Imobiliária Aurora", organizacao.Nome);
        Assert.Equal(Instante, organizacao.CriadaEm);
    }

    [Fact]
    public void Rehidratar_nao_registra_evento_de_dominio()
    {
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var organizacao = Organizacao.Rehidratar(id, "Imobiliária Aurora", ConfiguracaoDaOrganizacao.Padrao(), Instante, Instante);

        Assert.Empty(organizacao.EventosDeDominio);
    }

    [Fact]
    public void Renomear_substitui_o_nome_e_avanca_a_auditoria()
    {
        var autorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var organizacao = Organizacao.Fundar("Ana Souza", new RelogioFixo(Instante)).Valor;
        var depois = Instante.AddDays(1);

        var resultado = organizacao.Renomear("Imobiliária Aurora Ltda", autorId, new RelogioFixo(depois));

        Assert.True(resultado.Sucesso);
        Assert.Equal("Imobiliária Aurora Ltda", organizacao.Nome);
        Assert.Equal(depois, organizacao.Auditoria.AtualizadoEm);
        Assert.Equal(Instante, organizacao.CriadaEm);
    }

    [Fact]
    public void Renomear_registra_evento_com_nome_antigo_novo_e_autor()
    {
        var autorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var organizacao = Organizacao.Fundar("Ana Souza", new RelogioFixo(Instante)).Valor;
        organizacao.LimparEventos();

        organizacao.Renomear("Imobiliária Aurora Ltda", autorId, new RelogioFixo(Instante));

        var evento = Assert.Single(organizacao.EventosDeDominio.OfType<OrganizacaoRenomeadaEvento>());
        Assert.Equal(organizacao.Id, evento.OrganizacaoId);
        Assert.Equal("Ana Souza", evento.NomeAntigo);
        Assert.Equal("Imobiliária Aurora Ltda", evento.NomeNovo);
        Assert.Equal(autorId, evento.AutorId);
    }

    [Fact]
    public void Renomear_sem_nome_falha_com_nome_obrigatorio_e_nao_altera_nada()
    {
        var organizacao = Organizacao.Fundar("Ana Souza", new RelogioFixo(Instante)).Valor;

        var resultado = organizacao.Renomear("   ", Guid.NewGuid(), new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeOrganizacao.NomeObrigatorio, resultado.Erro);
        Assert.Equal("Ana Souza", organizacao.Nome);
    }
}
