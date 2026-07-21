using Morpheus.Api.Configuracao;
using Morpheus.Api.Identidade;
using Morpheus.Api.Observabilidade;
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
    construtor.Services.AddOpenApi();

    var aplicacao = construtor.Build();

    aplicacao.UsarObservabilidade();

    if (aplicacao.Environment.IsDevelopment())
        aplicacao.MapOpenApi();

    aplicacao.MapGet("/health", () => Results.Ok(new { status = "ok" }));

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
