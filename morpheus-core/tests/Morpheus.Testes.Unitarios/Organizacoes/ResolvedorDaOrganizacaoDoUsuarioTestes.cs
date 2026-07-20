using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

public sealed class ResolvedorDaOrganizacaoDoUsuarioTestes
{
    private static readonly Guid Usuario = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Organizacao = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Retorna_do_cache_sem_consultar_o_banco()
    {
        var cache = new CacheDeOrganizacaoDoUsuarioFake();
        await cache.ArmazenarAsync(Usuario, Organizacao, CancellationToken.None);
        var consulta = new ConsultaDaOrganizacaoDoUsuarioFake();
        var resolvedor = new ResolvedorDaOrganizacaoDoUsuario(cache, consulta);

        var resultado = await resolvedor.ResolverAsync(Usuario, CancellationToken.None);

        Assert.Equal(Organizacao, resultado);
        Assert.Equal(0, consulta.QuantidadeDeChamadas);
    }

    [Fact]
    public async Task Em_miss_consulta_o_banco_e_popula_o_cache_uma_vez()
    {
        var cache = new CacheDeOrganizacaoDoUsuarioFake();
        var consulta = new ConsultaDaOrganizacaoDoUsuarioFake();
        consulta.Vincular(Usuario, Organizacao);
        var resolvedor = new ResolvedorDaOrganizacaoDoUsuario(cache, consulta);

        var resultado = await resolvedor.ResolverAsync(Usuario, CancellationToken.None);

        Assert.Equal(Organizacao, resultado);
        Assert.Equal(1, consulta.QuantidadeDeChamadas);
        Assert.Equal(1, cache.QuantidadeDeArmazenamentos);
    }

    [Fact]
    public async Task Falha_quando_usuario_nao_tem_organizacao_citando_o_usuario()
    {
        var resolvedor = new ResolvedorDaOrganizacaoDoUsuario(
            new CacheDeOrganizacaoDoUsuarioFake(),
            new ConsultaDaOrganizacaoDoUsuarioFake());

        var erro = await Assert.ThrowsAsync<ErroDeOrganizacaoDoUsuarioNaoEncontrada>(
            () => resolvedor.ResolverAsync(Usuario, CancellationToken.None));

        Assert.Equal(Usuario, erro.UsuarioId);
    }

    [Fact]
    public async Task Rejeita_usuario_vazio()
    {
        var resolvedor = new ResolvedorDaOrganizacaoDoUsuario(
            new CacheDeOrganizacaoDoUsuarioFake(),
            new ConsultaDaOrganizacaoDoUsuarioFake());

        await Assert.ThrowsAsync<ArgumentException>(
            () => resolvedor.ResolverAsync(Guid.Empty, CancellationToken.None));
    }
}
