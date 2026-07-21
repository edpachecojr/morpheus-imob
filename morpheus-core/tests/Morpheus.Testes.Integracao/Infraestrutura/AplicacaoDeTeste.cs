using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Api.Configuracao;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Sobe o host real da API (o mesmo <c>Program</c> de produção) apontando o banco
/// para o Postgres do container de teste e trocando apenas a origem da identidade:
/// o contexto HTTP dá lugar ao <see cref="ContextoDoUsuarioDeTeste"/>, dirigido
/// pelos testes. Todo o resto — filtro, resolvedor, cache, outbox, Npgsql — é o
/// grafo de produção, para que o isolamento seja provado como ele roda.
/// </summary>
public sealed class AplicacaoDeTeste : WebApplicationFactory<Program>
{
    private readonly string _stringDeConexao;

    public AplicacaoDeTeste(string stringDeConexao) => _stringDeConexao = stringDeConexao;

    protected override void ConfigureWebHost(IWebHostBuilder construtor)
    {
        construtor.UseSetting(VariaveisDeAmbienteObrigatorias.ChaveDaConexao, _stringDeConexao);
        construtor.ConfigureTestServices(servicos =>
        {
            servicos.AddSingleton<IdentidadeDeTesteAtual>();
            servicos.RemoveAll<IContextoDoUsuario>();
            servicos.AddScoped<IContextoDoUsuario, ContextoDoUsuarioDeTeste>();
        });
    }
}
