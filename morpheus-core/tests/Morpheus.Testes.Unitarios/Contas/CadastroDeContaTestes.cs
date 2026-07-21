using Morpheus.Aplicacao.Contas;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Contas;

/// <summary>
/// Cobre as regras do cadastro que não dependem de banco: a fundação da
/// organização junto do dono, a resposta indistinguível para e-mail já cadastrado
/// e a ordem das validações que impede o formulário de virar oráculo de contas.
/// </summary>
public sealed class CadastroDeContaTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
    private const string SenhaValida = "uma-senha-bem-longa";

    private readonly ExecucaoTransacionalFake _transacao = new();
    private readonly RepositorioDeOrganizacoesFake _organizacoes = new();
    private readonly RegistroDeUsuariosFake _usuarios = new();
    private readonly TokensDeConfirmacaoDeEmailFake _tokensDeConfirmacao = new();
    private readonly EnvioDeEmailDeConfirmacaoFake _envioDeConfirmacao = new();

    [Fact]
    public async Task Funda_a_organizacao_com_o_dono_na_mesma_transacao()
    {
        var resultado = await Executar(Cadastro());

        Assert.True(resultado.Sucesso);
        Assert.True(_transacao.Confirmou);
        var organizacao = Assert.Single(_organizacoes.Adicionadas);
        var dono = Assert.Single(_usuarios.Criados);
        Assert.Equal(organizacao.Id, dono.OrganizacaoId);
        Assert.Equal(PapeisDoUsuario.Dono, dono.Papel);
    }

    [Fact]
    public async Task Cadastro_bem_sucedido_emite_e_envia_confirmacao_de_email()
    {
        await Executar(Cadastro());

        var envio = Assert.Single(_envioDeConfirmacao.Envios);
        Assert.Equal("ana@exemplo.com", envio.Email);
        Assert.Equal("Ana Souza", envio.NomeCompleto);
        Assert.Equal(Assert.Single(_tokensDeConfirmacao.Emitidos), envio.Token);
    }

    [Fact]
    public async Task Email_ja_cadastrado_nao_envia_confirmacao()
    {
        _usuarios.JaCadastrado("ana@exemplo.com");

        await Executar(Cadastro());

        Assert.Empty(_envioDeConfirmacao.Envios);
    }

    [Fact]
    public async Task Organizacao_nasce_com_o_nome_do_fundador_e_configuracao_padrao()
    {
        await Executar(Cadastro());

        var organizacao = Assert.Single(_organizacoes.Adicionadas);
        Assert.Equal("Ana Souza", organizacao.Nome);
        Assert.Equal(ConfiguracaoDaOrganizacao.FusoHorarioPadrao, organizacao.Configuracao.FusoHorario);
        Assert.Equal(JanelaDeAtendimento.Padrao, organizacao.Configuracao.JanelaDeAtendimento);
    }

    [Fact]
    public async Task Email_ja_cadastrado_responde_sucesso_sem_criar_nada()
    {
        _usuarios.JaCadastrado("ana@exemplo.com");

        var resultado = await Executar(Cadastro());

        Assert.True(resultado.Sucesso);
        Assert.Empty(_organizacoes.Adicionadas);
        Assert.Equal(0, _usuarios.TentativasDeCriacao);
    }

    [Fact]
    public async Task Senha_fraca_e_recusada_antes_de_consultar_a_existencia_do_email()
    {
        _usuarios.JaCadastrado("ana@exemplo.com");
        _usuarios.SenhaRecusadaCom = ErrosDeCadastro.SenhaRecusada("curta demais");

        var resultado = await Executar(Cadastro());

        // Se a existência viesse antes, e-mail cadastrado devolveria sucesso e
        // e-mail novo devolveria 400 — a diferença que denuncia quem tem conta.
        Assert.True(resultado.Falha);
        Assert.Equal("Cadastro.SenhaRecusada", resultado.Erro.Codigo);
    }

    [Fact]
    public async Task Corrida_pelo_mesmo_email_desfaz_a_transacao_e_responde_sucesso()
    {
        _usuarios.CriacaoRecusadaCom = ErrosDeCadastro.EmailJaCadastrado;

        var resultado = await Executar(Cadastro());

        Assert.True(resultado.Sucesso);
        Assert.True(_transacao.Desfez);
    }

    [Fact]
    public async Task Falha_ao_criar_o_usuario_desfaz_a_organizacao()
    {
        _usuarios.CriacaoRecusadaCom = ErrosDeCadastro.CadastroRecusado("store indisponível");

        var resultado = await Executar(Cadastro());

        Assert.True(resultado.Falha);
        Assert.True(_transacao.Desfez);
    }

    [Fact]
    public async Task Entrada_invalida_nem_chega_a_abrir_transacao()
    {
        var resultado = await Executar(new DadosDoCadastro("Ana Souza", "ana@exemplo.com", SenhaValida, "outra"));

        Assert.Equal(ErrosDeCadastro.SenhasNaoConferem, resultado.Erro);
        Assert.False(_transacao.Confirmou);
        Assert.False(_transacao.Desfez);
    }

    private static DadosDoCadastro Cadastro()
        => new("Ana Souza", "ana@exemplo.com", SenhaValida, SenhaValida);

    private Task<Morpheus.Dominio.Resultados.Resultado> Executar(DadosDoCadastro dados)
        => new CadastroDeConta(
                _transacao, _organizacoes, _usuarios, _tokensDeConfirmacao, _envioDeConfirmacao, new RelogioFixo(Instante))
            .ExecutarAsync(dados, CancellationToken.None);
}
