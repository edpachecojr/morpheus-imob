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
        organizacao.Property(o => o.CriadaEm).IsRequired();
    }
}
