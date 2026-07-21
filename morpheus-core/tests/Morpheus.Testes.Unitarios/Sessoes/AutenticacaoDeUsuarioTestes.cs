using Morpheus.Aplicacao.Sessoes;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Sessoes;

/// <summary>
/// Cobre o login (E1-F2-H1) com foco no que é difícil de ver por fora: os três
/// modos de recusa gastam o mesmo trabalho de derivação de chave, e por isso não
/// dá para distinguir e-mail inexistente de senha errada pelo tempo de resposta.
/// </summary>
public sealed class AutenticacaoDeUsuarioTestes
{
    private const string Email = "ana@exemplo.com";
    private const string Senha = "uma-senha-bem-longa";

    private readonly DiretorioDeUsuariosFake _usuarios = new();
    private readonly VerificadorDeSenhaFake _senhas = new();
    private readonly SessaoDoPainelFake _sessao = new();

    [Fact]
    public async Task Credenciais_validas_abrem_a_sessao_e_zeram_as_falhas()
    {
        var usuario = _usuarios.Registrar(Email);
        _senhas.DefinirSenha(usuario.Id, Senha);

        var resultado = await Autenticar(Email, Senha);

        Assert.True(resultado.Sucesso);
        Assert.Equal([usuario.Id], _sessao.Abertas);
        Assert.Equal(1, _senhas.LimpezasDeFalhas);
    }

    [Fact]
    public async Task Senha_errada_recusa_registra_falha_e_nao_abre_sessao()
    {
        var usuario = _usuarios.Registrar(Email);
        _senhas.DefinirSenha(usuario.Id, Senha);

        var resultado = await Autenticar(Email, "senha-errada-longa");

        Assert.Equal(ErrosDeAutenticacao.SenhaIncorreta, resultado.Erro);
        Assert.Equal(1, _senhas.FalhasRegistradas);
        Assert.Empty(_sessao.Abertas);
    }

    [Fact]
    public async Task Email_inexistente_recusa_sem_contabilizar_falha_em_conta_alheia()
    {
        var resultado = await Autenticar("ninguem@exemplo.com", Senha);

        Assert.Equal(ErrosDeAutenticacao.ContaInexistente, resultado.Erro);
        Assert.Equal(0, _senhas.FalhasRegistradas);
    }

    [Fact]
    public async Task Conta_bloqueada_recusa_sem_conferir_a_senha()
    {
        var usuario = _usuarios.Registrar(Email, bloqueado: true);
        _senhas.DefinirSenha(usuario.Id, Senha);

        var resultado = await Autenticar(Email, Senha);

        Assert.Equal(ErrosDeAutenticacao.ContaBloqueada, resultado.Erro);
        Assert.Empty(_sessao.Abertas);
    }

    [Fact]
    public async Task Os_tres_modos_de_recusa_derivam_a_mesma_quantidade_de_chaves()
    {
        var comSenhaErrada = await ContarDerivacoes(usuarios =>
        {
            var usuario = usuarios.Registrar(Email);
            return (Email, "senha-errada-longa", usuario);
        });

        var semConta = await ContarDerivacoes(_ => ("ninguem@exemplo.com", Senha, null));

        var bloqueada = await ContarDerivacoes(usuarios =>
        {
            var usuario = usuarios.Registrar(Email, bloqueado: true);
            return (Email, Senha, usuario);
        });

        Assert.Equal(1, comSenhaErrada);
        Assert.Equal(comSenhaErrada, semConta);
        Assert.Equal(comSenhaErrada, bloqueada);
    }

    private Task<Morpheus.Dominio.Resultados.Resultado> Autenticar(string email, string senha)
        => new AutenticacaoDeUsuario(_usuarios, _senhas, _sessao).ExecutarAsync(email, senha, CancellationToken.None);

    private static async Task<int> ContarDerivacoes(
        Func<DiretorioDeUsuariosFake, (string Email, string Senha, Morpheus.Aplicacao.Usuarios.UsuarioDoPainel? Usuario)> preparar)
    {
        var usuarios = new DiretorioDeUsuariosFake();
        var senhas = new VerificadorDeSenhaFake();
        var (email, senha, usuario) = preparar(usuarios);
        if (usuario is not null)
            senhas.DefinirSenha(usuario.Id, "outra-senha-qualquer");

        await new AutenticacaoDeUsuario(usuarios, senhas, new SessaoDoPainelFake())
            .ExecutarAsync(email, senha, CancellationToken.None);

        return senhas.DerivacoesDeChave;
    }
}
