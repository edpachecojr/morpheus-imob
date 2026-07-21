using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Morpheus.Aplicacao.Sessoes;

namespace Morpheus.Api.Identidade;

/// <summary>
/// Compõe a autenticação do painel: cookie <c>HttpOnly</c>/<c>Secure</c>/
/// <c>SameSite=Lax</c> apoiado num armazenamento de sessões no servidor
/// (ADR-0011). Sem redirecionamento para tela de login — esta é uma API, e cliente
/// sem sessão precisa de 401, não de um 302 para HTML.
/// </summary>
public static class ConfiguracaoDeAutenticacao
{
    /// <summary>Nome do esquema de autenticação do painel.</summary>
    public const string EsquemaDeSessao = "morpheus.sessao";

    private const string NomeDoCookie = "morpheus_sessao";
    private static readonly TimeSpan ValidadeDaSessao = TimeSpan.FromDays(14);

    public static IServiceCollection AdicionarAutenticacaoPorSessao(
        this IServiceCollection servicos, bool ehDesenvolvimento)
    {
        servicos.AddSingleton<ITicketStore, TicketStoreDeSessoes>();
        servicos.AddScoped<ISessaoDoPainel, SessaoDoPainelComCookie>();

        servicos.AddAuthentication(EsquemaDeSessao)
                .AddCookie(EsquemaDeSessao, opcoes => Configurar(opcoes, ehDesenvolvimento));

        // O armazenamento entra por DI e não pelo delegate acima, que não recebe
        // provedor de serviços.
        servicos.AddOptions<CookieAuthenticationOptions>(EsquemaDeSessao)
                .Configure<ITicketStore>((opcoes, armazenamento) => opcoes.SessionStore = armazenamento);

        return servicos;
    }

    private static void Configurar(CookieAuthenticationOptions opcoes, bool ehDesenvolvimento)
    {
        opcoes.Cookie.Name = NomeDoCookie;
        opcoes.Cookie.HttpOnly = true;
        opcoes.Cookie.SameSite = SameSiteMode.Lax;

        // Em desenvolvimento a API roda em http; exigir Secure ali faria o
        // navegador descartar o cookie e o login "não funcionar" sem dizer por quê.
        opcoes.Cookie.SecurePolicy = ehDesenvolvimento
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        opcoes.ExpireTimeSpan = ValidadeDaSessao;
        opcoes.SlidingExpiration = true;

        opcoes.Events.OnRedirectToLogin = ResponderCom(StatusCodes.Status401Unauthorized);
        opcoes.Events.OnRedirectToAccessDenied = ResponderCom(StatusCodes.Status403Forbidden);
    }

    private static Func<RedirectContext<CookieAuthenticationOptions>, Task> ResponderCom(int status) =>
        contexto =>
        {
            contexto.Response.StatusCode = status;
            return Task.CompletedTask;
        };
}
