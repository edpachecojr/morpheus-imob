using Microsoft.Extensions.DependencyInjection;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Base dos testes de integração: dá acesso ao ambiente compartilhado e concentra
/// os utilitários de sessão (executar como um usuário ou sem sessão) e de
/// semeadura, para que cada teste fique curto e legível.
/// </summary>
public abstract class TesteDeIntegracao
{
    protected AmbienteDeIntegracao Ambiente { get; }

    protected TesteDeIntegracao(AmbienteDeIntegracao ambiente) => Ambiente = ambiente;

    /// <summary>Executa a ação num escopo autenticado como o usuário dado.</summary>
    protected async Task<T> ComoUsuario<T>(Guid usuarioId, Func<IServiceProvider, Task<T>> acao)
    {
        Ambiente.Autenticar(usuarioId);
        try
        {
            using var escopo = Ambiente.Aplicacao.Services.CreateScope();
            return await acao(escopo.ServiceProvider);
        }
        finally
        {
            Ambiente.EncerrarSessao();
        }
    }

    protected Task ComoUsuario(Guid usuarioId, Func<IServiceProvider, Task> acao)
        => ComoUsuario(usuarioId, async provedor => { await acao(provedor); return 0; });

    /// <summary>Executa a ação num escopo sem usuário autenticado (caminho job/bootstrap).</summary>
    protected async Task SemSessao(Func<IServiceProvider, Task> acao)
    {
        Ambiente.EncerrarSessao();
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        await acao(escopo.ServiceProvider);
    }

    /// <summary>
    /// Cria uma organização com um usuário dono vinculado, devolvendo os ids. Cada
    /// teste usa organizações novas, então dados de testes anteriores no mesmo
    /// container jamais interferem no resultado.
    /// </summary>
    protected async Task<OrganizacaoSemeada> SemearOrganizacaoAsync(string nome)
    {
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>();

        var organizacao = new Organizacao(nome, TimeProvider.System);
        banco.Organizacoes.Add(organizacao);
        await banco.SaveChangesAsync();

        var usuarioId = await CriarDonoAsync(banco, organizacao.Id, nome);
        return new OrganizacaoSemeada(organizacao.Id, usuarioId);
    }

    /// <summary>
    /// Cadastra um imóvel já vinculado à organização, pelo caminho sem sessão
    /// (vínculo explícito), isolando a semeadura da lógica de contexto autenticado.
    /// </summary>
    protected async Task SemearImovelAsync(Guid organizacaoId, string codigo, string endereco)
    {
        Ambiente.EncerrarSessao();
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>();

        var imovel = new Imovel(codigo, endereco, TimeProvider.System);
        imovel.AtribuirOrganizacao(organizacaoId);
        banco.Imoveis.Add(imovel);
        await banco.SaveChangesAsync();
    }

    private static async Task<Guid> CriarDonoAsync(MorpheusDbContext banco, Guid organizacaoId, string nome)
    {
        var login = $"dono-{Guid.NewGuid():N}@exemplo.test";
        var dono = new UsuarioDaOrganizacao { Id = Guid.NewGuid(), UserName = login, Email = login };
        dono.AtribuirOrganizacao(organizacaoId);
        dono.DefinirPapel(PapelDoUsuario.Dono);
        dono.DefinirNomeCompleto($"Dono {nome}");

        banco.Users.Add(dono);
        await banco.SaveChangesAsync();
        return dono.Id;
    }
}

/// <summary>Ids resultantes da semeadura de uma organização e seu usuário dono.</summary>
public sealed record OrganizacaoSemeada(Guid OrganizacaoId, Guid UsuarioId);
