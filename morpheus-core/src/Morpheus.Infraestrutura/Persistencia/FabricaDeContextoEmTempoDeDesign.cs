using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Fábrica usada apenas pelas ferramentas de migração (<c>dotnet ef</c>). Lê a
/// conexão de <c>MORPHEUS_BANCO_CONEXAO</c>; na ausência dela, cai num alvo
/// local padrão só para gerar/comparar o modelo — nunca serve a produção, e o
/// valor é o mesmo dummy do docker-compose de desenvolvimento.
/// </summary>
public sealed class FabricaDeContextoEmTempoDeDesign : IDesignTimeDbContextFactory<MorpheusDbContext>
{
    private const string ConexaoLocalDeDesenvolvimento =
        "Host=localhost;Port=5432;Database=morpheus;Username=morpheus;Password=morpheus";

    public MorpheusDbContext CreateDbContext(string[] argumentos)
    {
        var conexao = Environment.GetEnvironmentVariable("MORPHEUS_BANCO_CONEXAO")
            ?? ConexaoLocalDeDesenvolvimento;

        var opcoes = new DbContextOptionsBuilder<MorpheusDbContext>()
            .UseNpgsql(conexao)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new MorpheusDbContext(opcoes);
    }
}
