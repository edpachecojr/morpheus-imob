using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morpheus.Infraestrutura.Persistencia.Outbox;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ConfiguracaoDeMensagemDeOutbox : IEntityTypeConfiguration<MensagemDeOutbox>
{
    public void Configure(EntityTypeBuilder<MensagemDeOutbox> mensagem)
    {
        mensagem.ToTable("mensagens_outbox");
        mensagem.HasKey(m => m.Id);
        mensagem.Property(m => m.OrganizacaoId).IsRequired();
        mensagem.Property(m => m.TipoDoEvento).HasMaxLength(120).IsRequired();

        // jsonb, não text: deixa o payload consultável e valida o JSON no banco.
        mensagem.Property(m => m.Conteudo).HasColumnType("jsonb").IsRequired();
        mensagem.Property(m => m.OcorridoEm).IsRequired();

        // Índice do scan de pendentes que o futuro dispatcher fará (processado_em
        // IS NULL ORDER BY ocorrido_em) — cria-se agora para não migrar depois.
        mensagem.HasIndex(m => new { m.ProcessadoEm, m.OcorridoEm });

        // O outbox NÃO é IPertenceOrganizacao: grava a organização como dado do
        // envelope, não como vínculo de tenant. Não há FK para organizacoes de
        // propósito — a mensagem sobrevive à remoção do agregado.
        mensagem.HasIndex(m => m.OrganizacaoId);
    }
}
