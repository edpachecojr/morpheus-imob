using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Usuarios;

/// <summary>
/// Cobre o reenvio de confirmação de e-mail (E1-F2-H6): emite um token novo,
/// invalidando o anterior, e envia ao usuário da sessão corrente.
/// </summary>
public sealed class SolicitacaoDeConfirmacaoDeEmailTestes
{
    private readonly DiretorioDeUsuariosFake _usuarios = new();
    private readonly TokensDeConfirmacaoDeEmailFake _tokens = new();
    private readonly EnvioDeEmailDeConfirmacaoFake _envio = new();

    [Fact]
    public async Task Reenvio_emite_token_novo_e_envia_ao_usuario_da_sessao()
    {
        var usuario = _usuarios.Registrar("ana@exemplo.com");

        await Executar(usuario.Id);

        var envio = Assert.Single(_envio.Envios);
        Assert.Equal("ana@exemplo.com", envio.Email);
        Assert.Equal(Assert.Single(_tokens.Emitidos), envio.Token);
    }

    [Fact]
    public async Task Reenvio_invalida_o_token_anterior()
    {
        var usuario = _usuarios.Registrar("ana@exemplo.com");
        var tokenAntigo = await _tokens.GerarAsync(usuario.Id, CancellationToken.None);

        await Executar(usuario.Id);

        var confirmacaoComOAntigo = await _tokens.ConfirmarAsync(usuario.Id, tokenAntigo, CancellationToken.None);
        Assert.True(confirmacaoComOAntigo.Falha);
    }

    [Fact]
    public async Task Sem_sessao_lanca_erro_de_usuario_nao_autenticado()
    {
        var caso = new SolicitacaoDeConfirmacaoDeEmail(
            ContextoDoUsuarioFake.SemSessao(), _usuarios, _tokens, _envio);

        await Assert.ThrowsAsync<ErroDeUsuarioNaoAutenticado>(() => caso.ExecutarAsync(CancellationToken.None));
    }

    private Task Executar(Guid usuarioId)
        => new SolicitacaoDeConfirmacaoDeEmail(ContextoDoUsuarioFake.Autenticado(usuarioId), _usuarios, _tokens, _envio)
            .ExecutarAsync(CancellationToken.None);
}
