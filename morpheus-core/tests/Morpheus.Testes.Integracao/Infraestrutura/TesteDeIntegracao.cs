using Microsoft.AspNetCore.Identity;
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
    /// <summary>
    /// Senha das contas semeadas. Não é segredo de sistema nenhum: vale só dentro
    /// do container efêmero do teste (CLAUDE.md §Segredos).
    /// </summary>
    protected const string SenhaDeTeste = "senha-de-teste-longa";

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

    /// <summary>Consulta o banco fora de qualquer sessão, para conferir o estado gravado.</summary>
    protected async Task<T> NoBanco<T>(Func<MorpheusDbContext, Task<T>> consulta)
    {
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        return await consulta(escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>());
    }

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
        var organizacaoId = await FundarOrganizacaoAsync(nome);
        var dono = await SemearUsuarioAsync(organizacaoId, PapeisDoUsuario.Dono, $"Dono {nome}");
        return new OrganizacaoSemeada(organizacaoId, dono.Id, dono.Email);
    }

    /// <summary>
    /// Cria um usuário da organização com o papel pedido, pelo mesmo
    /// <c>UserManager</c> da produção — o vínculo com o papel vai para
    /// <c>user_roles</c>, e não para uma coluna inventada pelo teste.
    /// </summary>
    protected async Task<UsuarioSemeado> SemearUsuarioAsync(Guid organizacaoId, string papel, string nome)
    {
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var registro = escopo.ServiceProvider.GetRequiredService<UserManager<UsuarioDaOrganizacao>>();

        var login = $"{papel}-{Guid.NewGuid():N}@exemplo.test";
        var usuario = UsuarioDaOrganizacao.Cadastrar(
            new OrganizacaoDona(organizacaoId), nome, login, papel, TimeProvider.System);

        Confirmar(await registro.CreateAsync(usuario, SenhaDeTeste));
        Confirmar(await registro.AddToRoleAsync(usuario, papel));
        return new UsuarioSemeado(usuario.Id, login);
    }

    /// <summary>
    /// Cadastra um imóvel já vinculado à organização, pelo caminho sem sessão,
    /// isolando a semeadura da lógica de contexto autenticado. O tenant entra na
    /// própria factory, como manda o domínio.
    /// </summary>
    protected async Task SemearImovelAsync(Guid organizacaoId, string codigo, string endereco)
    {
        Ambiente.EncerrarSessao();
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>();

        var imovel = Imovel.Cadastrar(new OrganizacaoDona(organizacaoId), codigo, endereco, TimeProvider.System).Valor;
        banco.Imoveis.Add(imovel);
        await banco.SaveChangesAsync();
    }

    private async Task<Guid> FundarOrganizacaoAsync(string nome)
    {
        using var escopo = Ambiente.Aplicacao.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>();

        var organizacao = Organizacao.Fundar(nome, TimeProvider.System).Valor;
        banco.Organizacoes.Add(organizacao);
        await banco.SaveChangesAsync();
        return organizacao.Id;
    }

    // Semeadura que falha em silêncio produz teste que passa por acidente.
    private static void Confirmar(IdentityResult resultado)
    {
        if (!resultado.Succeeded)
            throw new InvalidOperationException(
                $"Semeadura de usuário falhou: {string.Join(" ", resultado.Errors.Select(erro => erro.Description))}");
    }
}

/// <summary>Ids resultantes da semeadura de uma organização e seu usuário dono.</summary>
public sealed record OrganizacaoSemeada(Guid OrganizacaoId, Guid UsuarioId, string EmailDoDono);

/// <summary>Identificação de um usuário semeado, com o login gerado para ele.</summary>
public sealed record UsuarioSemeado(Guid Id, string Email);
