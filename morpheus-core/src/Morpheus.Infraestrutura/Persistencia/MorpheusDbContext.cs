using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Infraestrutura.Persistencia.Configuracoes;
using Morpheus.Infraestrutura.Persistencia.Outbox;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Contexto EF Core do sistema, estendendo o store do IdentityCore. NÃO declara
/// query filter global de organização: o isolamento é imposto por filtro
/// explícito no repositório de leitura e pelo interceptor de escrita — decisão
/// registrada no ADR-0003 (defesa estrutural, não disciplina em cada query).
/// </summary>
public sealed class MorpheusDbContext : IdentityDbContext<UsuarioDaOrganizacao, IdentityRole<Guid>, Guid>
{
    public MorpheusDbContext(DbContextOptions<MorpheusDbContext> opcoes) : base(opcoes)
    {
    }

    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();
    public DbSet<Imovel> Imoveis => Set<Imovel>();
    public DbSet<MensagemDeOutbox> MensagensDeOutbox => Set<MensagemDeOutbox>();

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        base.OnModelCreating(construtor);
        construtor.ApplyConfiguration(new ConfiguracaoDeOrganizacao());
        construtor.ApplyConfiguration(new ConfiguracaoDeImovel());
        construtor.ApplyConfiguration(new ConfiguracaoDeMensagemDeOutbox());
        construtor.ApplyConfiguration(new ConfiguracaoDeUsuarioDaOrganizacao());
        ConfiguracaoDasTabelasDaIdentidade.Aplicar(construtor);
    }
}
