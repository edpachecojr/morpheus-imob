using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;
using Morpheus.Infraestrutura.Persistencia.Outbox;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Outbox;

/// <summary>
/// Prova o lado de escrita do outbox contra um Postgres real: toda ação de escrita
/// grava o evento na tabela <c>mensagens_outbox</c>, na MESMA transação do dado —
/// a resiliência que garante que dado e evento sobem ou caem juntos. A drenagem
/// (dispatcher/filas) está fora de escopo neste MVP ([ADR-0009]).
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class GravacaoDoOutboxTestes : TesteDeIntegracao
{
    public GravacaoDoOutboxTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Fundar_organizacao_grava_evento_no_outbox_com_o_tenant()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");

        var mensagens = await MensagensDaOrganizacaoAsync(aurora.OrganizacaoId);

        var mensagem = Assert.Single(mensagens, m => m.TipoDoEvento == nameof(OrganizacaoFundadaEvento));
        Assert.Equal(aurora.OrganizacaoId, mensagem.OrganizacaoId);
        Assert.Contains("Aurora", mensagem.Conteudo);
        Assert.Null(mensagem.ProcessadoEm);
    }

    [Fact]
    public async Task Cadastrar_imovel_grava_evento_no_outbox_com_dados_do_imovel()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        await SemearImovelAsync(aurora.OrganizacaoId, "AP-101", "Rua Aurora, 101");

        var mensagens = await MensagensDaOrganizacaoAsync(aurora.OrganizacaoId);

        var mensagem = Assert.Single(mensagens, m => m.TipoDoEvento == nameof(ImovelCadastradoEvento));
        Assert.Equal(aurora.OrganizacaoId, mensagem.OrganizacaoId);
        Assert.Contains("AP-101", mensagem.Conteudo);
        Assert.Contains("Rua Aurora, 101", mensagem.Conteudo);
    }

    [Fact]
    public async Task Escrita_rejeitada_nao_deixa_evento_orfao_no_outbox()
    {
        var aurora = await SemearOrganizacaoAsync("Aurora");
        var belaVista = await SemearOrganizacaoAsync("Bela Vista");

        // Escrita com organização divergente do contexto é rejeitada antes do commit.
        await Assert.ThrowsAsync<ErroDeOrganizacaoDivergente>(() =>
            ComoUsuario(aurora.UsuarioId, async provedor =>
            {
                var banco = provedor.GetRequiredService<MorpheusDbContext>();
                var intruso = Imovel.Cadastrar("AP-999", "Rua Intrusa, 999", TimeProvider.System).Valor;
                intruso.AtribuirOrganizacao(belaVista.OrganizacaoId);
                banco.Imoveis.Add(intruso);
                await banco.SaveChangesAsync();
            }));

        // Nenhum evento de imóvel vazou: dado e evento compartilham a transação.
        var mensagens = await MensagensDaOrganizacaoAsync(belaVista.OrganizacaoId);
        Assert.DoesNotContain(mensagens, m => m.TipoDoEvento == nameof(ImovelCadastradoEvento));
    }

    private async Task<IReadOnlyList<MensagemDeOutbox>> MensagensDaOrganizacaoAsync(Guid organizacaoId)
    {
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>();
        return await banco.MensagensDeOutbox
            .AsNoTracking()
            .Where(m => m.OrganizacaoId == organizacaoId)
            .ToListAsync();
    }
}
