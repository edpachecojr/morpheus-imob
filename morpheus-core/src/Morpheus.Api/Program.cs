using Morpheus.Api.Autorizacao;
using Morpheus.Api.Configuracao;
using Morpheus.Api.Endpoints;
using Morpheus.Api.Erros;
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

    construtor.Services.AdicionarInfraestrutura(stringDeConexao, construtor.Configuration);
    construtor.Services.AddHttpContextAccessor();
    construtor.Services.AddScoped<IContextoDoUsuario, ContextoDoUsuarioHttp>();
    construtor.Services.AdicionarAutenticacaoPorSessao(construtor.Environment.IsDevelopment());
    construtor.Services.AdicionarAutorizacaoPorPermissao();
    construtor.Services.AdicionarLimiteDeAutenticacao(construtor.Configuration);
    construtor.Services.AddExceptionHandler<ManipuladorGlobalDeExcecoes>();

    // UseExceptionHandler() exige o serviço de ProblemDetails registrado para
    // existir, mesmo com um IExceptionHandler próprio. O enriquecimento padrão
    // (traceId por requisição) sai: quebraria a resposta byte-idêntica que a
    // autenticação usa contra enumeração de contas (RespostaDeFalha também
    // passa por este serviço, não só o manipulador de exceção).
    construtor.Services.AddProblemDetails(
        opcoes => opcoes.CustomizeProblemDetails = contexto => contexto.ProblemDetails.Extensions.Remove("traceId"));
    construtor.Services.AddOpenApi();

    var aplicacao = construtor.Build();

    // Primeiro middleware: precisa envolver todo o resto do pipeline para
    // capturar exceção não tratada de qualquer rota (E1-F4-H2).
    aplicacao.UseExceptionHandler();

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
