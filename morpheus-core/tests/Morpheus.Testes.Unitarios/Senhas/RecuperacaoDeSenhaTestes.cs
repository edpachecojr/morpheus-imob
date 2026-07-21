using Morpheus.Aplicacao.Senhas;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Senhas;

/// <summary>
/// Cobre a recuperação e a redefinição de senha (E1-F2-H3): silêncio para e-mail
/// inexistente, token de uso único e queda de todas as sessões na troca.
/// </summary>
public sealed class RecuperacaoDeSenhaTestes
{
    private const string Email = "ana@exemplo.com";
    private const string NovaSenha = "outra-senha-bem-longa";

    private readonly DiretorioDeUsuariosFake _usuarios = new();
    private readonly TokensDeRecuperacaoFake _tokens = new();
    private readonly EnvioDeEmailDeRecuperacaoFake _envio = new();
    private readonly SessaoDoPainelFake _sessao = new();

    [Fact]
    public async Task Conta_existente_recebe_o_link()
    {
        _usuarios.Registrar(Email);

        await Solicitar(Email);

        var envio = Assert.Single(_envio.Envios);
        Assert.Equal(Email, envio.Email);
        Assert.Equal(Assert.Single(_tokens.Emitidos), envio.Token);
    }

    [Fact]
    public async Task Email_inexistente_nao_emite_token_nem_envia_nada()
    {
        await Solicitar("ninguem@exemplo.com");

        Assert.Empty(_tokens.Emitidos);
        Assert.Empty(_envio.Envios);
    }

    [Fact]
    public async Task Token_valido_redefine_a_senha_e_derruba_as_outras_sessoes()
    {
        var usuario = _usuarios.Registrar(Email);
        await Solicitar(Email);

        var resultado = await Redefinir(Email, _tokens.Emitidos[0]);

        Assert.True(resultado.Sucesso);
        Assert.Equal([usuario.Id], _sessao.RevogadasPorUsuario);
    }

    [Fact]
    public async Task Token_ja_usado_e_recusado_na_segunda_vez()
    {
        _usuarios.Registrar(Email);
        await Solicitar(Email);
        var token = _tokens.Emitidos[0];
        await Redefinir(Email, token);

        var segunda = await Redefinir(Email, token);

        Assert.Equal(ErrosDeAutenticacao.TokenDeRecuperacaoInvalido, segunda.Erro);
    }

    [Fact]
    public async Task Email_inexistente_na_redefinicao_devolve_token_invalido_sem_revelar_a_conta()
    {
        var resultado = await Redefinir("ninguem@exemplo.com", "token-0");

        Assert.Equal(ErrosDeAutenticacao.TokenDeRecuperacaoInvalido, resultado.Erro);
        Assert.Empty(_sessao.RevogadasPorUsuario);
    }

    private Task Solicitar(string email)
        => new SolicitacaoDeRecuperacaoDeSenha(_usuarios, _tokens, _envio)
            .ExecutarAsync(email, CancellationToken.None);

    private Task<Morpheus.Dominio.Resultados.Resultado> Redefinir(string email, string token)
        => new RedefinicaoDeSenha(_usuarios, _tokens, _sessao)
            .ExecutarAsync(email, token, NovaSenha, CancellationToken.None);
}
