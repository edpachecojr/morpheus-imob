using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ConfiguracaoDeImovel : IEntityTypeConfiguration<Imovel>
{
    public void Configure(EntityTypeBuilder<Imovel> imovel)
    {
        imovel.ToTable("imoveis");
        imovel.HasKey(i => i.Id);
        imovel.Property(i => i.CodigoDeReferencia).HasMaxLength(60).IsRequired();
        imovel.Property(i => i.Endereco).HasMaxLength(300).IsRequired();
        imovel.Property(i => i.OrganizacaoId).IsRequired();

        // Auditoria mora nas colunas do próprio imóvel (owned, sem tabela à parte).
        ConfiguracaoDeAuditoria.Mapear(imovel);

        // CadastradoEm é projeção de Auditoria.CriadoEm; eventos são transientes.
        imovel.Ignore(i => i.CadastradoEm);
        imovel.Ignore(i => i.EventosDeDominio);

        // Índice do vínculo de organização: acelera o filtro presente em toda leitura.
        imovel.HasIndex(i => i.OrganizacaoId);

        // Código de referência único DENTRO da organização (E1-F1-H3): dois
        // tenants podem usar "AP-101" sem conflito.
        imovel.HasIndex(i => new { i.OrganizacaoId, i.CodigoDeReferencia }).IsUnique();

        // FK nunca cruza organização; a integridade é do banco, não da aplicação.
        imovel.HasOne<Organizacao>()
              .WithMany()
              .HasForeignKey(i => i.OrganizacaoId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
