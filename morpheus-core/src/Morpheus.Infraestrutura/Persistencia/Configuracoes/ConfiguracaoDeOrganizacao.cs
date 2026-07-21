using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ConfiguracaoDeOrganizacao : IEntityTypeConfiguration<Organizacao>
{
    public void Configure(EntityTypeBuilder<Organizacao> organizacao)
    {
        organizacao.ToTable("organizacoes");
        organizacao.HasKey(o => o.Id);
        organizacao.Property(o => o.Nome).HasMaxLength(200).IsRequired();

        // Auditoria mora nas colunas da própria organização (owned, sem tabela à parte).
        ConfiguracaoDeAuditoria.Mapear(organizacao);

        // CriadaEm é projeção de Auditoria.CriadoEm; eventos são transientes.
        organizacao.Ignore(o => o.CriadaEm);
        organizacao.Ignore(o => o.EventosDeDominio);
    }
}
