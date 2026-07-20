using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

public sealed class ContextoDaOrganizacaoAtualTestes
{
    private static readonly Guid Usuario = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Organizacao = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task Sem_usuario_autenticado_a_versao_estrita_falha()
    {
        var contexto = new ContextoDaOrganizacaoAtual(
            ContextoDoUsuarioFake.SemSessao(),
            new ResolvedorDaOrganizacaoDoUsuarioFake(Organizacao));

        await Assert.ThrowsAsync<ErroDeUsuarioNaoAutenticado>(
            () => contexto.ObterOrganizacaoIdAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Sem_usuario_autenticado_a_versao_opcional_devolve_nulo()
    {
        var contexto = new ContextoDaOrganizacaoAtual(
            ContextoDoUsuarioFake.SemSessao(),
            new ResolvedorDaOrganizacaoDoUsuarioFake(Organizacao));

        var resultado = await contexto.ObterOrganizacaoIdOuNuloAsync(CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Com_usuario_autenticado_retorna_a_organizacao_resolvida()
    {
        var contexto = new ContextoDaOrganizacaoAtual(
            ContextoDoUsuarioFake.Autenticado(Usuario),
            new ResolvedorDaOrganizacaoDoUsuarioFake(Organizacao));

        var resultado = await contexto.ObterOrganizacaoIdAsync(CancellationToken.None);

        Assert.Equal(Organizacao, resultado);
    }
}
