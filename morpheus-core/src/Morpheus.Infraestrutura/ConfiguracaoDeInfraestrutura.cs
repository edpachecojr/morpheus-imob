using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Morpheus.Aplicacao.Autorizacao;
using Morpheus.Aplicacao.Comum;
using Morpheus.Aplicacao.Contas;
using Morpheus.Aplicacao.Imoveis;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Aplicacao.Senhas;
using Morpheus.Aplicacao.Sessoes;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Infraestrutura.Email;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Infraestrutura.Imoveis;
using Morpheus.Infraestrutura.Observabilidade;
using Morpheus.Infraestrutura.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;
using Morpheus.Infraestrutura.Persistencia.Outbox;
using Morpheus.Infraestrutura.Sessoes;

namespace Morpheus.Infraestrutura;

/// <summary>
/// Composição da infraestrutura: banco, identidade, cache, isolamento por
/// organização, sessões e acesso a imóveis. Um único ponto para montar o grafo de
/// dependências de dados, mantendo o host (API/worker) magro.
/// </summary>
public static class ConfiguracaoDeInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        string stringDeConexao,
        IConfiguration configuracao)
    {
        servicos.AddSingleton(TimeProvider.System);
        servicos.AddMemoryCache();

        AdicionarBanco(servicos, stringDeConexao);
        AdicionarIdentidade(servicos);
        AdicionarEmailTransacional(servicos, configuracao);
        AdicionarIsolamentoPorOrganizacao(servicos);
        AdicionarSessoes(servicos, stringDeConexao);
        AdicionarCasosDeUsoDeConta(servicos);
        AdicionarAcessoAImoveis(servicos, stringDeConexao);

        return servicos;
    }

    private static void AdicionarBanco(IServiceCollection servicos, string stringDeConexao)
    {
        AdicionarOutbox(servicos);

        // O MontadorDeMensagensDeOutbox chega ao contexto por injeção de construtor:
        // o SaveChanges do próprio MorpheusDbContext drena os eventos para o outbox.
        servicos.AddDbContext<MorpheusDbContext>(opcoes =>
            opcoes.UseNpgsql(stringDeConexao)
                  .UseSnakeCaseNamingConvention());

        servicos.AddScoped<IExecucaoTransacional, ExecucaoTransacionalComEfCore>();
    }

    private static void AdicionarOutbox(IServiceCollection servicos)
    {
        servicos.AddSingleton<ISerializadorDeEvento, SerializadorDeEventoComSystemTextJson>();
        servicos.AddScoped<MontadorDeMensagensDeOutbox>();
    }

    private static void AdicionarIdentidade(IServiceCollection servicos)
    {
        servicos.AddIdentityCore<UsuarioDaOrganizacao>(DefinirPoliticaDeIdentidade)
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<MorpheusDbContext>()
                .AddDefaultTokenProviders();

        // Token de redefinição vale 30 minutos (fundacao/autenticacao.md); o padrão
        // do Identity é um dia, longo demais para credencial que anda por e-mail.
        servicos.Configure<DataProtectionTokenProviderOptions>(
            opcoes => opcoes.TokenLifespan = TimeSpan.FromMinutes(30));

        // Replace, não Add: o AddIdentityCore acima já registrou o PBKDF2 padrão, e
        // trocá-lo por Argon2id é exigência de fundacao/autenticacao.md.
        servicos.Replace(
            ServiceDescriptor.Singleton<IPasswordHasher<UsuarioDaOrganizacao>, HashDeSenhaArgon2id>());
        servicos.AddSingleton<SenhaDeReferenciaParaEqualizarTempo>();

        AdicionarServicosDeIdentidade(servicos);
    }

    private static void AdicionarServicosDeIdentidade(IServiceCollection servicos)
    {
        servicos.AddScoped<IRegistroDeUsuarios, RegistroDeUsuariosComIdentity>();
        servicos.AddScoped<IDiretorioDeUsuarios, DiretorioDeUsuariosComIdentity>();
        servicos.AddScoped<IAtualizacaoDeUsuario, AtualizacaoDeUsuarioComIdentity>();
        servicos.AddScoped<IVerificadorDeSenha, VerificadorDeSenhaComIdentity>();
        servicos.AddScoped<ITokensDeRecuperacaoDeSenha, TokensDeRecuperacaoComIdentity>();
        servicos.AddScoped<ITokensDeConfirmacaoDeEmail, TokensDeConfirmacaoDeEmailComIdentity>();
        servicos.AddSingleton<IAutorizadorDeAcesso, AutorizadorPorClaimDePermissao>();
    }

    private static void AdicionarEmailTransacional(IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.Configure<ConfiguracaoDeEmailTransacional>(
            configuracao.GetSection(ConfiguracaoDeEmailTransacional.Secao));

        servicos.AddScoped<RemetenteDeEmailComSmtp>();
        servicos.AddScoped<IEnvioDeEmailDeRecuperacao, EnvioDeRecuperacaoPorEmail>();
        servicos.AddScoped<IEnvioDeEmailDeConfirmacao, EnvioDeConfirmacaoPorEmail>();
    }

    // Comprimento acima de zoológico de símbolos: o NIST 800-63B abandonou a regra
    // de complexidade porque ela produz senha curta e previsível ("Senha@123").
    private static void DefinirPoliticaDeIdentidade(IdentityOptions opcoes)
    {
        opcoes.User.RequireUniqueEmail = true;

        opcoes.Password.RequiredLength = 12;
        opcoes.Password.RequireDigit = false;
        opcoes.Password.RequireLowercase = false;
        opcoes.Password.RequireUppercase = false;
        opcoes.Password.RequireNonAlphanumeric = false;

        // Rate limit por conta: cinco erros bloqueiam por 15 minutos. O limite por
        // IP é do host, em ConfiguracaoDeLimiteDeRequisicoes.
        opcoes.Lockout.AllowedForNewUsers = true;
        opcoes.Lockout.MaxFailedAccessAttempts = 5;
        opcoes.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    }

    private static void AdicionarIsolamentoPorOrganizacao(IServiceCollection servicos)
    {
        servicos.AddSingleton<ICacheDeOrganizacaoDoUsuario, CacheDeOrganizacaoEmMemoria>();
        servicos.AddScoped<IConsultaDaOrganizacaoDoUsuario, ConsultaDaOrganizacaoDoUsuarioComDapper>();
        servicos.AddScoped<IResolvedorDaOrganizacaoDoUsuario, ResolvedorDaOrganizacaoDoUsuario>();
        servicos.AddScoped<IContextoDaOrganizacaoAtual, ContextoDaOrganizacaoAtual>();
        servicos.AddScoped<IRepositorioDeOrganizacoes, RepositorioDeOrganizacoesComEfCore>();
        servicos.AddScoped<RenomeacaoDaOrganizacao>();
    }

    // Singleton porque a sessão é restaurada na autenticação do cookie, antes de
    // existir escopo de requisição de onde tirar um DbContext.
    private static void AdicionarSessoes(IServiceCollection servicos, string stringDeConexao)
        => servicos.AddSingleton<IArmazenamentoDeSessoes>(provedor =>
            new ArmazenamentoDeSessoesComDapper(
                new FabricaDeConexaoNpgsql(stringDeConexao),
                provedor.GetRequiredService<TimeProvider>()));

    private static void AdicionarCasosDeUsoDeConta(IServiceCollection servicos)
    {
        servicos.AddScoped<CadastroDeConta>();
        servicos.AddScoped<AutenticacaoDeUsuario>();
        servicos.AddScoped<SolicitacaoDeRecuperacaoDeSenha>();
        servicos.AddScoped<RedefinicaoDeSenha>();
        servicos.AddScoped<AtualizacaoDeDadosDoUsuario>();
        servicos.AddScoped<SolicitacaoDeConfirmacaoDeEmail>();
        servicos.AddScoped<ConfirmacaoDeEmail>();
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
