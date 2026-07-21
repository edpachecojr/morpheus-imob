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

        MapearConfiguracaoOperacional(organizacao);
        MapearMetadados(organizacao);
    }

    // Configuração operacional (fuso e janela de atendimento) mora nas colunas
    // da própria organização: é 1-para-1 e nunca consultada sem ela.
    private static void MapearConfiguracaoOperacional(EntityTypeBuilder<Organizacao> organizacao)
    {
        organizacao.OwnsOne(o => o.Configuracao, configuracao =>
        {
            configuracao.Property(c => c.FusoHorario)
                        .HasColumnName("fuso_horario").HasMaxLength(60).IsRequired();
            configuracao.OwnsOne(c => c.JanelaDeAtendimento, janela =>
            {
                janela.Property(j => j.Inicio).HasColumnName("atendimento_inicio").IsRequired();
                janela.Property(j => j.Fim).HasColumnName("atendimento_fim").IsRequired();
            });
            configuracao.Navigation(c => c.JanelaDeAtendimento).IsRequired();
        });
        organizacao.Navigation(o => o.Configuracao).IsRequired();
    }

    private static void MapearMetadados(EntityTypeBuilder<Organizacao> organizacao)
    {
        // Auditoria mora nas colunas da própria organização (owned, sem tabela à parte).
        ConfiguracaoDeAuditoria.Mapear(organizacao);

        // CriadaEm é projeção de Auditoria.CriadoEm; eventos são transientes.
        organizacao.Ignore(o => o.CriadaEm);
        organizacao.Ignore(o => o.EventosDeDominio);
    }
}
