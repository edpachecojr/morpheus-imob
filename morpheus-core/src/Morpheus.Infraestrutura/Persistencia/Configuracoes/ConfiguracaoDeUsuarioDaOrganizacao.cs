using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Identidade;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ConfiguracaoDeUsuarioDaOrganizacao : IEntityTypeConfiguration<UsuarioDaOrganizacao>
{
    public void Configure(EntityTypeBuilder<UsuarioDaOrganizacao> usuario)
    {
        // Substitui o nome padrão "AspNetUsers" do IdentityCore pela convenção do
        // schema (plural, snake_case). As demais tabelas do Identity são mapeadas
        // em ConfiguracaoDasTabelasDaIdentidade.
        usuario.ToTable("usuarios");

        usuario.Property(u => u.OrganizacaoId).IsRequired();
        usuario.Property(u => u.NomeCompleto).HasMaxLength(200).IsRequired();

        // Papel NÃO é coluna daqui: mora em user_roles, do próprio Identity
        // (ADR-0010). Eventos de domínio são transientes — o outbox os drena
        // antes do commit.
        usuario.Ignore(u => u.EventosDeDominio);

        MapearVinculoComOrganizacao(usuario);
    }

    private static void MapearVinculoComOrganizacao(EntityTypeBuilder<UsuarioDaOrganizacao> usuario)
    {
        // Índice do vínculo: a busca "organização do usuário" roda em toda
        // requisição (antes do cache aquecer) e precisa ser barata.
        usuario.HasIndex(u => u.OrganizacaoId);

        usuario.HasOne<Organizacao>()
               .WithMany()
               .HasForeignKey(u => u.OrganizacaoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
