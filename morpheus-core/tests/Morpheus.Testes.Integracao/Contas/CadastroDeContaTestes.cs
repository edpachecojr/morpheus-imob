using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;
using Morpheus.Testes.Integracao.Infraestrutura;

namespace Morpheus.Testes.Integracao.Contas;

/// <summary>
/// Prova o cadastro de conta e tenant contra Postgres real (E1-F1-H2): a
/// organização e o dono entram na mesma transação, com papel do Identity e
/// configuração padrão, e os três caminhos que não podem se distinguir — criado,
/// e-mail repetido e robô — devolvem exatamente a mesma resposta.
/// </summary>
[Collection(ColecaoDeIntegracao.NomeDaColecao)]
public sealed class CadastroDeContaTestes : TesteDeIntegracao
{
    private const string SenhaForte = "uma-senha-bem-longa";

    public CadastroDeContaTestes(AmbienteDeIntegracao ambiente) : base(ambiente) { }

    [Fact]
    public async Task Cadastro_cria_organizacao_e_dono_na_mesma_transacao()
    {
        var email = EmailNovo();

        var resposta = await Cadastrar(email, "Ana Souza");

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var usuario = await BuscarUsuarioAsync(email);
        Assert.NotNull(usuario);
        Assert.NotEqual(Guid.Empty, usuario.OrganizacaoId);
        Assert.Equal("Ana Souza", usuario.NomeCompleto);
    }

    [Fact]
    public async Task Dono_recebe_o_papel_dono_nas_tabelas_do_identity()
    {
        var email = EmailNovo();

        await Cadastrar(email, "Bruno Lima");

        var usuario = await BuscarUsuarioAsync(email);
        var papeis = await NoBanco(banco =>
            banco.UserRoles.Where(vinculo => vinculo.UserId == usuario!.Id)
                 .Join(banco.Roles, vinculo => vinculo.RoleId, papel => papel.Id, (_, papel) => papel.Name)
                 .ToListAsync());

        Assert.Equal([PapeisDoUsuario.Dono], papeis);
    }

    [Fact]
    public async Task Organizacao_nasce_com_o_nome_do_fundador_e_configuracao_utilizavel()
    {
        var email = EmailNovo();

        await Cadastrar(email, "Carla Dias");

        var usuario = await BuscarUsuarioAsync(email);
        var organizacao = await NoBanco(banco =>
            banco.Organizacoes.SingleAsync(o => o.Id == usuario!.OrganizacaoId));

        Assert.Equal("Carla Dias", organizacao.Nome);
        Assert.Equal(ConfiguracaoDaOrganizacao.FusoHorarioPadrao, organizacao.Configuracao.FusoHorario);
        Assert.Equal(new TimeOnly(9, 0), organizacao.Configuracao.JanelaDeAtendimento.Inicio);
        Assert.Equal(new TimeOnly(18, 0), organizacao.Configuracao.JanelaDeAtendimento.Fim);
    }

    [Fact]
    public async Task Cadastro_grava_os_eventos_de_fundacao_e_de_usuario_no_outbox()
    {
        var email = EmailNovo();

        await Cadastrar(email, "Diego Reis");

        var usuario = await BuscarUsuarioAsync(email);
        var tipos = await NoBanco(banco =>
            banco.MensagensDeOutbox
                 .Where(mensagem => mensagem.OrganizacaoId == usuario!.OrganizacaoId)
                 .Select(mensagem => mensagem.TipoDoEvento)
                 .ToListAsync());

        Assert.Contains(nameof(OrganizacaoFundadaEvento), tipos);
        Assert.Contains(nameof(UsuarioCadastradoEvento), tipos);
    }

    [Fact]
    public async Task Email_repetido_responde_igual_ao_cadastro_novo_e_nao_cria_segunda_organizacao()
    {
        var email = EmailNovo();
        var primeira = await Cadastrar(email, "Elis Prado");
        var organizacoesAntes = await ContarOrganizacoesAsync();

        var repetida = await Cadastrar(email, "Impostor");

        Assert.Equal(primeira.StatusCode, repetida.StatusCode);
        Assert.Equal(await primeira.Content.ReadAsStringAsync(), await repetida.Content.ReadAsStringAsync());
        Assert.Equal(organizacoesAntes, await ContarOrganizacoesAsync());
    }

    [Fact]
    public async Task Armadilha_de_robo_preenchida_responde_igual_sem_criar_conta()
    {
        var email = EmailNovo();
        var legitima = await Cadastrar(EmailNovo(), "Fabio Nunes");

        var doRobo = await Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/contas", new
        {
            nomeCompleto = "Robo",
            email,
            senha = SenhaForte,
            confirmacaoDeSenha = SenhaForte,
            paginaPessoal = "https://spam.exemplo",
        });

        Assert.Equal(legitima.StatusCode, doRobo.StatusCode);
        Assert.Equal(await legitima.Content.ReadAsStringAsync(), await doRobo.Content.ReadAsStringAsync());
        Assert.Null(await BuscarUsuarioAsync(email));
    }

    [Fact]
    public async Task Confirmacao_de_senha_diferente_e_recusada()
    {
        var resposta = await Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/contas", new
        {
            nomeCompleto = "Gil Souza",
            email = EmailNovo(),
            senha = SenhaForte,
            confirmacaoDeSenha = "outra-senha-longa",
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Senha_curta_e_recusada_antes_de_qualquer_escrita()
    {
        var email = EmailNovo();

        var resposta = await Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/contas", new
        {
            nomeCompleto = "Helo Braga",
            email,
            senha = "curta",
            confirmacaoDeSenha = "curta",
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Null(await BuscarUsuarioAsync(email));
    }

    private Task<HttpResponseMessage> Cadastrar(string email, string nome) =>
        Ambiente.Aplicacao.CreateClient().PostAsJsonAsync("/contas", new
        {
            nomeCompleto = nome,
            email,
            senha = SenhaForte,
            confirmacaoDeSenha = SenhaForte,
        });

    private static string EmailNovo() => $"cadastro-{Guid.NewGuid():N}@exemplo.test";

    private Task<Morpheus.Infraestrutura.Identidade.UsuarioDaOrganizacao?> BuscarUsuarioAsync(string email) =>
        NoBanco(banco => banco.Users.AsNoTracking()
            .SingleOrDefaultAsync(usuario => usuario.NormalizedEmail == email.ToUpperInvariant()));

    private Task<int> ContarOrganizacoesAsync() => NoBanco(banco => banco.Organizacoes.CountAsync());
}
