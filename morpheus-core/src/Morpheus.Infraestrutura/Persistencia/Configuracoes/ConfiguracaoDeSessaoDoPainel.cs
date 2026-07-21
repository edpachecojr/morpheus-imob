using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morpheus.Infraestrutura.Identidade;
using Morpheus.Infraestrutura.Sessoes;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ConfiguracaoDeSessaoDoPainel : IEntityTypeConfiguration<SessaoDoPainelArmazenada>
{
    public void Configure(EntityTypeBuilder<SessaoDoPainelArmazenada> sessao)
    {
        sessao.ToTable("sessoes");
        sessao.HasKey(s => s.Id);
        sessao.Property(s => s.Conteudo).IsRequired();
        sessao.Property(s => s.ExpiraEm).IsRequired();
        sessao.Property(s => s.CriadaEm).IsRequired();

        // Revogar todas as sessões de um usuário roda em toda troca de senha e é
        // um DELETE por usuario_id: sem este índice, varre a tabela inteira.
        sessao.HasIndex(s => s.UsuarioId);

        // Em cascata, ao contrário do resto do schema: usuário removido não deixa
        // sessão órfã capaz de restaurar uma identidade que não existe mais.
        sessao.HasOne<UsuarioDaOrganizacao>()
              .WithMany()
              .HasForeignKey(s => s.UsuarioId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
