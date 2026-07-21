using Morpheus.Dominio.Comum;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Persistencia.Outbox;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Outbox;

public sealed class MontadorDeMensagensDeOutboxTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly MontadorDeMensagensDeOutbox _montador = new(new SerializadorDeEventoFake());

    [Fact]
    public void Drenar_converte_evento_de_imovel_em_mensagem_com_o_tenant_da_entidade()
    {
        var imovel = ImovelDaOrganizacao();

        var mensagens = _montador.Drenar([imovel]);

        var mensagem = Assert.Single(mensagens);
        Assert.Equal(TenantId, mensagem.OrganizacaoId);
        Assert.Equal(nameof(ImovelCadastradoEvento), mensagem.TipoDoEvento);
        Assert.Equal(Instante, mensagem.OcorridoEm);
        Assert.Null(mensagem.ProcessadoEm);
    }

    [Fact]
    public void Drenar_usa_o_proprio_id_da_organizacao_como_tenant()
    {
        var organizacao = Organizacao.Fundar("Imobiliária Aurora", new RelogioFixo(Instante)).Valor;

        var mensagem = Assert.Single(_montador.Drenar([organizacao]));

        Assert.Equal(organizacao.Id, mensagem.OrganizacaoId);
        Assert.Equal(nameof(OrganizacaoFundadaEvento), mensagem.TipoDoEvento);
    }

    [Fact]
    public void Drenar_esvazia_os_eventos_da_entidade_para_nao_gravar_duas_vezes()
    {
        var imovel = ImovelDaOrganizacao();

        _montador.Drenar([imovel]);

        Assert.Empty(imovel.EventosDeDominio);
        Assert.Empty(_montador.Drenar([imovel]));
    }

    [Fact]
    public void Drenar_ignora_entidade_sem_eventos()
    {
        var semEventos = Imovel.Rehidratar(
            Guid.NewGuid(), TenantId, "AP-200", "Rua Sem Evento, 200", Instante, Instante);

        Assert.Empty(_montador.Drenar([semEventos]));
    }

    [Fact]
    public void Drenar_recusa_portadora_de_evento_sem_tenant_resolvivel()
    {
        // As entidades reais tornam isto impossível (tenant obrigatório na construção);
        // o ramo defensivo ainda recusa uma portadora que não expõe organização.
        var evento = new ImovelCadastradoEvento(Guid.NewGuid(), "AP-101", "Rua das Acácias, 100", Instante);
        var semTenant = new PortadoraDeEventoSemTenant(evento);

        Assert.Throws<InvalidOperationException>(() => _montador.Drenar([semTenant]));
    }

    private static Imovel ImovelDaOrganizacao() =>
        Imovel.Cadastrar(new OrganizacaoDona(TenantId), "AP-101", "Rua das Acácias, 100", new RelogioFixo(Instante)).Valor;
}
