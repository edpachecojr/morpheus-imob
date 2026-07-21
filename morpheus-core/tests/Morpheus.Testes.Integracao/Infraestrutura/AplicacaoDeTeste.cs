using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Morpheus.Api.Configuracao;
using Morpheus.Api.Identidade;
using Morpheus.Api.Seguranca;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Aplicacao.Senhas;
using Morpheus.Aplicacao.Usuarios;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Sobe o host real da API (o mesmo <c>Program</c> de produção) apontando o banco
/// para o Postgres do container de teste e acrescentando à identidade uma origem
/// dirigida pelos testes. Todo o resto — autenticação, autorização, filtro,
/// resolvedor, cache, outbox, Npgsql — é o grafo de produção, para que as
/// garantias sejam provadas como elas rodam.
/// </summary>
public sealed class AplicacaoDeTeste : WebApplicationFactory<Program>
{
    /// <summary>
    /// Teto de autenticação folgado para a suíte comum, que dispara dezenas de
    /// requisições da mesma origem. O teste do limite em si sobe um host próprio
    /// com um teto baixo.
    /// </summary>
    public const int LimiteFolgado = 1000;

    private readonly string _stringDeConexao;
    private readonly int _limiteDeAutenticacao;

    public AplicacaoDeTeste(string stringDeConexao, int limiteDeAutenticacao = LimiteFolgado)
    {
        _stringDeConexao = stringDeConexao;
        _limiteDeAutenticacao = limiteDeAutenticacao;
    }

    protected override void ConfigureWebHost(IWebHostBuilder construtor)
    {
        // Desenvolvimento para que o cookie de sessão não exija HTTPS: o cliente de
        // teste fala http, e um cookie Secure seria descartado antes de voltar.
        construtor.UseEnvironment("Development");
        construtor.UseSetting(VariaveisDeAmbienteObrigatorias.ChaveDaConexao, _stringDeConexao);
        construtor.UseSetting(
            $"{OpcoesDeLimiteDeAutenticacao.Secao}:{nameof(OpcoesDeLimiteDeAutenticacao.RequisicoesPorMinuto)}",
            _limiteDeAutenticacao.ToString(CultureInfo.InvariantCulture));

        construtor.ConfigureTestServices(servicos =>
        {
            servicos.AddSingleton<IdentidadeDeTesteAtual>();
            servicos.AddScoped<ContextoDoUsuarioHttp>();
            servicos.RemoveAll<IContextoDoUsuario>();
            servicos.AddScoped<IContextoDoUsuario, ContextoDoUsuarioDeTeste>();

            // Nenhum teste de integração fala SMTP de verdade: os dois envios de
            // e-mail transacional caem no mesmo dublê em memória.
            servicos.AddSingleton<EnviosDeEmailDeTeste>();
            servicos.RemoveAll<IEnvioDeEmailDeRecuperacao>();
            servicos.AddSingleton<IEnvioDeEmailDeRecuperacao>(
                provedor => provedor.GetRequiredService<EnviosDeEmailDeTeste>());
            servicos.RemoveAll<IEnvioDeEmailDeConfirmacao>();
            servicos.AddSingleton<IEnvioDeEmailDeConfirmacao>(
                provedor => provedor.GetRequiredService<EnviosDeEmailDeTeste>());
        });
    }
}
