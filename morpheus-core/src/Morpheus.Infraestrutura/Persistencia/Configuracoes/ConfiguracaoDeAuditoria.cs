using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morpheus.Dominio.Comum;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeia a <see cref="DadosDeAuditoria"/> como value object owned nas colunas
/// <c>criado_em</c> e <c>atualizado_em</c> da própria tabela da entidade — sem
/// tabela nem chave à parte. Centraliza o mapeamento para toda entidade auditada
/// carimbar as mesmas colunas com um só passo de configuração.
/// </summary>
internal static class ConfiguracaoDeAuditoria
{
    public static void Mapear<TEntidade>(EntityTypeBuilder<TEntidade> entidade)
        where TEntidade : EntidadeBase
    {
        entidade.OwnsOne(e => e.Auditoria, auditoria =>
        {
            auditoria.Property(a => a.CriadoEm).HasColumnName("criado_em").IsRequired();
            auditoria.Property(a => a.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();
        });
        entidade.Navigation(e => e.Auditoria).IsRequired();
    }
}
