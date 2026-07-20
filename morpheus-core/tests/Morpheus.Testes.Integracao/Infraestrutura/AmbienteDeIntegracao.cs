using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morpheus.Infraestrutura.Persistencia;
using Testcontainers.PostgreSql;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Fixture compartilhada da suíte de integração: sobe um PostgreSQL efêmero em
/// container, aplica as migrações reais e expõe a <see cref="AplicacaoDeTeste"/>
/// já configurada. Container real (não fake) é o ponto dos testes de integração —
/// só um Postgres de verdade prova as barreiras impostas pelo banco (FK e índice
/// único composto), que os testes unitários não alcançam.
/// </summary>
public sealed class AmbienteDeIntegracao : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public AplicacaoDeTeste Aplicacao { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        Aplicacao = new AplicacaoDeTeste(_postgres.GetConnectionString());
        await AplicarMigracoesAsync();
    }

    public async Task DisposeAsync()
    {
        await Aplicacao.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public void Autenticar(Guid usuarioId) => Identidade().Autenticar(usuarioId);

    public void EncerrarSessao() => Identidade().Encerrar();

    private IdentidadeDeTesteAtual Identidade()
        => Aplicacao.Services.GetRequiredService<IdentidadeDeTesteAtual>();

    private async Task AplicarMigracoesAsync()
    {
        using var escopo = Aplicacao.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<MorpheusDbContext>();
        await banco.Database.MigrateAsync();
    }
}

/// <summary>
/// Reúne os testes de integração numa única coleção: eles compartilham o mesmo
/// container e rodam em série, o que mantém cada teste independente sem custo de
/// subir um Postgres por classe.
/// </summary>
[CollectionDefinition(NomeDaColecao)]
public sealed class ColecaoDeIntegracao : ICollectionFixture<AmbienteDeIntegracao>
{
    public const string NomeDaColecao = "integracao-com-banco";
}
