using Morpheus.Api.Autorizacao;
using Morpheus.Api.Configuracao;
using Morpheus.Api.Endpoints;
using Morpheus.Api.Identidade;
using Morpheus.Api.Observabilidade;
using Morpheus.Api.Seguranca;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura;
using Serilog;

// Log mínimo antes do host existir, para não perder falha de subida (E1-F4-H1).
Log.Logger = ConfiguracaoDeObservabilidade.CriarLogInicial();

try
{
    var construtor = WebApplication.CreateBuilder(args);

    construtor.AdicionarObservabilidade();

    // Falha cedo se faltar configuração obrigatória, dizendo qual variável e o formato.
    var stringDeConexao = VariaveisDeAmbienteObrigatorias.LerStringDeConexao(construtor.Configuration);

    construtor.Services.AdicionarInfraestrutura(stringDeConexao);
    construtor.Services.AddHttpContextAccessor();
    construtor.Services.AddScoped<IContextoDoUsuario, ContextoDoUsuarioHttp>();
    construtor.Services.AdicionarAutenticacaoPorSessao(construtor.Environment.IsDevelopment());
    construtor.Services.AdicionarAutorizacaoPorPermissao();
    construtor.Services.AdicionarLimiteDeAutenticacao(construtor.Configuration);
    construtor.Services.AddOpenApi();

    var aplicacao = construtor.Build();

    aplicacao.UseRateLimiter();
    aplicacao.UseAuthentication();

    // Depois da autenticação: só com usuário resolvido o log carrega organizacao_id.
    aplicacao.UsarObservabilidade();
    aplicacao.UseAuthorization();

    if (aplicacao.Environment.IsDevelopment())
        aplicacao.MapOpenApi().AllowAnonymous();

    aplicacao.MapearEndpoints();

    aplicacao.Run();
}
catch (Exception excecao) when (excecao is not HostAbortedException)
{
    // HostAbortedException é o sinal normal da WebApplicationFactory nos testes.
    Log.Fatal(excecao, "Aplicação encerrou inesperadamente na subida");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Torna o Program acessível aos testes de integração (WebApplicationFactory).
public partial class Program;
