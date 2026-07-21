using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Infraestrutura.Imoveis;
using Morpheus.Infraestrutura.Observabilidade;
using Morpheus.Infraestrutura.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura;

/// <summary>
/// Composição da infraestrutura: banco, identidade, cache, isolamento por
/// organização e acesso a imóveis. Um único ponto para montar o grafo de
/// dependências de dados, mantendo o host (API/worker) magro.
/// </summary>
public static class ConfiguracaoDeInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        string stringDeConexao)
    {
        servicos.AddSingleton(TimeProvider.System);
        servicos.AddMemoryCache();

        AdicionarBanco(servicos, stringDeConexao);
        AdicionarIdentidade(servicos);
        AdicionarIsolamentoPorOrganizacao(servicos);
        AdicionarAcessoAImoveis(servicos, stringDeConexao);

        return servicos;
    }

    private static void AdicionarBanco(IServiceCollection servicos, string stringDeConexao)
    {
        servicos.AddScoped<InterceptorDeEscritaPorOrganizacao>();
        servicos.AddDbContext<MorpheusDbContext>((provedor, opcoes) =>
            opcoes.UseNpgsql(stringDeConexao)
                  .UseSnakeCaseNamingConvention()
                  .AddInterceptors(provedor.GetRequiredService<InterceptorDeEscritaPorOrganizacao>()));
    }

    private static void AdicionarIdentidade(IServiceCollection servicos)
    {
        servicos.AddIdentityCore<UsuarioDaOrganizacao>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<MorpheusDbContext>();
    }

    private static void AdicionarIsolamentoPorOrganizacao(IServiceCollection servicos)
    {
        servicos.AddSingleton<ICacheDeOrganizacaoDoUsuario, CacheDeOrganizacaoEmMemoria>();
        servicos.AddScoped<IConsultaDaOrganizacaoDoUsuario, ConsultaDaOrganizacaoDoUsuarioComDapper>();
        servicos.AddScoped<IResolvedorDaOrganizacaoDoUsuario, ResolvedorDaOrganizacaoDoUsuario>();
        servicos.AddScoped<IContextoDaOrganizacaoAtual, ContextoDaOrganizacaoAtual>();
    }

    private static void AdicionarAcessoAImoveis(IServiceCollection servicos, string stringDeConexao)
    {
        servicos.AddSingleton<IFabricaDeConexao>(new FabricaDeConexaoNpgsql(stringDeConexao));
        servicos.AddScoped<IRepositorioDeImoveis, RepositorioDeImoveisComEfCore>();
        servicos.AddScoped<IConsultaDeImoveisResumidos, LeitorDeImoveisComDapper>();

        // Log transversal por composição (OCP): o leitor Dapper não sabe que é observado.
        servicos.Decorar<IConsultaDeImoveisResumidos, ConsultaDeImoveisComRegistroDeLog>();
    }
}
