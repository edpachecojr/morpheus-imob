using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

/// <summary>
/// Cobre o onboarding da organização (E1-F1-H4): renomear substitui o nome do
/// fundador sem exigir nenhum outro dado, e o autor vem sempre da sessão, nunca
/// de parâmetro.
/// </summary>
public sealed class RenomeacaoDaOrganizacaoTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid AutorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly RepositorioDeOrganizacoesFake _organizacoes = new();

    [Fact]
    public async Task Renomeia_a_organizacao_do_contexto_e_persiste()
    {
        var organizacao = await SemearOrganizacaoAsync("Ana Souza");

        var resultado = await Executar(organizacao.Id, "Imobiliária Aurora Ltda");

        Assert.True(resultado.Sucesso);
        Assert.Equal("Imobiliária Aurora Ltda", organizacao.Nome);
        Assert.Same(organizacao, Assert.Single(_organizacoes.Atualizadas));
    }

    [Fact]
    public async Task Nome_vazio_falha_sem_persistir()
    {
        var organizacao = await SemearOrganizacaoAsync("Ana Souza");

        var resultado = await Executar(organizacao.Id, "   ");

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeOrganizacao.NomeObrigatorio, resultado.Erro);
        Assert.Empty(_organizacoes.Atualizadas);
    }

    private async Task<Organizacao> SemearOrganizacaoAsync(string nome)
    {
        var organizacao = Organizacao.Fundar(nome, new RelogioFixo(Instante)).Valor;
        await _organizacoes.AdicionarAsync(organizacao, CancellationToken.None);
        return organizacao;
    }

    private Task<Morpheus.Dominio.Resultados.Resultado> Executar(Guid organizacaoId, string novoNome)
        => new RenomeacaoDaOrganizacao(
                ContextoDaOrganizacaoAtualFake.Com(organizacaoId),
                ContextoDoUsuarioFake.Autenticado(AutorId),
                _organizacoes,
                new RelogioFixo(Instante))
            .ExecutarAsync(novoNome, CancellationToken.None);
}
