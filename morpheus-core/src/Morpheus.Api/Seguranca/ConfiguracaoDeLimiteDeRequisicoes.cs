using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Morpheus.Api.Seguranca;

/// <summary>
/// Limite de requisições por origem nas rotas de autenticação — a camada por IP
/// exigida em [autenticacao.md](../../../../docs/fundacao/autenticacao.md). A
/// camada por conta é o bloqueio por tentativas do Identity; as duas juntas
/// cobrem tanto o ataque distribuído numa conta quanto a varredura de contas a
/// partir de uma origem.
/// </summary>
public static class ConfiguracaoDeLimiteDeRequisicoes
{
    /// <summary>Nome da política aplicada a cadastro, login e recuperação de senha.</summary>
    public const string PoliticaDeAutenticacao = "autenticacao";

    public static IServiceCollection AdicionarLimiteDeAutenticacao(
        this IServiceCollection servicos, IConfiguration configuracao)
    {
        servicos.Configure<OpcoesDeLimiteDeAutenticacao>(
            configuracao.GetSection(OpcoesDeLimiteDeAutenticacao.Secao));

        servicos.AddRateLimiter(opcoes =>
        {
            opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opcoes.AddPolicy(PoliticaDeAutenticacao, Particionar);
        });

        return servicos;
    }

    private static RateLimitPartition<string> Particionar(HttpContext contexto)
    {
        var teto = contexto.RequestServices
            .GetRequiredService<IOptions<OpcoesDeLimiteDeAutenticacao>>().Value.RequisicoesPorMinuto;

        return RateLimitPartition.GetFixedWindowLimiter(OrigemDaRequisicao(contexto), _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = teto,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
    }

    // O IP é chave de partição, jamais campo de log: endereço é dado pessoal
    // (CLAUDE.md §Logging).
    private static string OrigemDaRequisicao(HttpContext contexto)
        => contexto.Connection.RemoteIpAddress?.ToString() ?? "origem-desconhecida";
}
