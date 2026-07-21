using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Usuarios;

/// <summary>
/// Cobre "completar meus dados" do onboarding (E1-F1-H4): o usuário da sessão
/// edita o próprio nome, sem depender de permissão nomeada.
/// </summary>
public sealed class AtualizacaoDeDadosDoUsuarioTestes
{
    private static readonly Guid UsuarioId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly AtualizacaoDeUsuarioFake _atualizacao = new();

    [Fact]
    public async Task Atualiza_o_nome_do_usuario_da_sessao()
    {
        var resultado = await Executar("Ana Souza");

        Assert.True(resultado.Sucesso);
        Assert.Equal("Ana Souza", _atualizacao.NomesAtualizados[UsuarioId]);
    }

    [Fact]
    public async Task Nome_vazio_falha_com_nome_obrigatorio()
    {
        var resultado = await Executar("   ");

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeUsuario.NomeCompletoObrigatorio, resultado.Erro);
    }

    [Fact]
    public async Task Sem_sessao_lanca_erro_de_usuario_nao_autenticado()
    {
        var caso = new AtualizacaoDeDadosDoUsuario(ContextoDoUsuarioFake.SemSessao(), _atualizacao);

        await Assert.ThrowsAsync<ErroDeUsuarioNaoAutenticado>(
            () => caso.ExecutarAsync("Ana Souza", CancellationToken.None));
    }

    private Task<Morpheus.Dominio.Resultados.Resultado> Executar(string nomeCompleto)
        => new AtualizacaoDeDadosDoUsuario(ContextoDoUsuarioFake.Autenticado(UsuarioId), _atualizacao)
            .ExecutarAsync(nomeCompleto, CancellationToken.None);
}
