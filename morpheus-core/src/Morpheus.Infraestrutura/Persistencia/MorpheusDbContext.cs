using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Morpheus.Dominio.Comum;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Infraestrutura.Persistencia.Configuracoes;
using Morpheus.Infraestrutura.Persistencia.Outbox;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Contexto EF Core do sistema, estendendo o store do IdentityCore. NÃO declara
/// query filter global de organização: o isolamento de leitura é imposto por filtro
/// explícito no repositório (ADR-0003) e o vínculo de escrita é definido na
/// construção da entidade, não aqui.
/// <para>
/// A gravação transacional do outbox é feita por override explícito de
/// <see cref="SaveChanges(bool)"/>/<see cref="SaveChangesAsync(bool, CancellationToken)"/>
/// — dado e evento sobem ou caem na mesma transação (ADR-0009). É deliberadamente
/// visível aqui, e não escondido num interceptor registrado longe, no contêiner.
/// </para>
/// </summary>
public sealed class MorpheusDbContext : IdentityDbContext<UsuarioDaOrganizacao, IdentityRole<Guid>, Guid>
{
    private readonly MontadorDeMensagensDeOutbox _montadorDeOutbox;

    public MorpheusDbContext(
        DbContextOptions<MorpheusDbContext> opcoes, MontadorDeMensagensDeOutbox montadorDeOutbox) : base(opcoes)
        => _montadorDeOutbox = montadorDeOutbox;

    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();
    public DbSet<Imovel> Imoveis => Set<Imovel>();
    public DbSet<MensagemDeOutbox> MensagensDeOutbox => Set<MensagemDeOutbox>();

    public override int SaveChanges(bool aceitarTodasAsMudancasAoConcluir)
    {
        GravarEventosNoOutbox();
        return base.SaveChanges(aceitarTodasAsMudancasAoConcluir);
    }

    public override Task<int> SaveChangesAsync(
        bool aceitarTodasAsMudancasAoConcluir, CancellationToken cancelamento = default)
    {
        GravarEventosNoOutbox();
        return base.SaveChangesAsync(aceitarTodasAsMudancasAoConcluir, cancelamento);
    }

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        base.OnModelCreating(construtor);
        construtor.ApplyConfiguration(new ConfiguracaoDeOrganizacao());
        construtor.ApplyConfiguration(new ConfiguracaoDeImovel());
        construtor.ApplyConfiguration(new ConfiguracaoDeMensagemDeOutbox());
        construtor.ApplyConfiguration(new ConfiguracaoDeUsuarioDaOrganizacao());
        ConfiguracaoDasTabelasDaIdentidade.Aplicar(construtor);
    }

    // Drena os eventos das entidades rastreadas para o outbox ANTES do commit, para
    // que as linhas de mensagem entrem na mesma transação do dado que as originou.
    private void GravarEventosNoOutbox()
    {
        var portadoras = ChangeTracker.Entries<IPossuiEventosDeDominio>().Select(entrada => entrada.Entity);
        var mensagens = _montadorDeOutbox.Drenar(portadoras);
        if (mensagens.Count > 0)
            MensagensDeOutbox.AddRange(mensagens);
    }
}
