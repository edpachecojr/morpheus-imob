using Morpheus.Api.Configuracao;
using Morpheus.Api.Identidade;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura;

var construtor = WebApplication.CreateBuilder(args);

// Log em JSON estruturado para depuração e busca; prosa é inútil para agentes (E1-F4-H1).
construtor.Logging.ClearProviders();
construtor.Logging.AddJsonConsole();

// Falha cedo se faltar configuração obrigatória, dizendo qual variável e o formato.
var stringDeConexao = VariaveisDeAmbienteObrigatorias.LerStringDeConexao(construtor.Configuration);

construtor.Services.AdicionarInfraestrutura(stringDeConexao);
construtor.Services.AddHttpContextAccessor();
construtor.Services.AddScoped<IContextoDoUsuario, ContextoDoUsuarioHttp>();
construtor.Services.AddOpenApi();

var aplicacao = construtor.Build();

if (aplicacao.Environment.IsDevelopment())
    aplicacao.MapOpenApi();

aplicacao.MapGet("/health", () => Results.Ok(new { status = "ok" }));

aplicacao.Run();

// Torna o Program acessível aos testes de integração (WebApplicationFactory).
public partial class Program;
