using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Usuarios;

/// <summary>
/// Cobre a confirmação de e-mail pelo link (E1-F2-H6): token válido confirma,
/// token inválido e e-mail sem conta devolvem o mesmo erro genérico.
/// </summary>
public sealed class ConfirmacaoDeEmailTestes
{
    private readonly DiretorioDeUsuariosFake _usuarios = new();
    private readonly TokensDeConfirmacaoDeEmailFake _tokens = new();

    [Fact]
    public async Task Token_valido_confirma_o_email()
    {
        var usuario = _usuarios.Registrar("ana@exemplo.com");
        var token = await _tokens.GerarAsync(usuario.Id, CancellationToken.None);

        var resultado = await Executar("ana@exemplo.com", token);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task Token_ja_usado_e_recusado_na_segunda_vez()
    {
        var usuario = _usuarios.Registrar("ana@exemplo.com");
        var token = await _tokens.GerarAsync(usuario.Id, CancellationToken.None);
        await Executar("ana@exemplo.com", token);

        var resultado = await Executar("ana@exemplo.com", token);

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeAutenticacao.TokenDeConfirmacaoInvalido, resultado.Erro);
    }

    [Fact]
    public async Task Email_sem_conta_devolve_o_mesmo_erro_de_token_invalido()
    {
        var resultado = await Executar("ninguem@exemplo.com", "token-qualquer");

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeAutenticacao.TokenDeConfirmacaoInvalido, resultado.Erro);
    }

    private Task<Morpheus.Dominio.Resultados.Resultado> Executar(string email, string token)
        => new ConfirmacaoDeEmail(_usuarios, _tokens).ExecutarAsync(email, token, CancellationToken.None);
}
