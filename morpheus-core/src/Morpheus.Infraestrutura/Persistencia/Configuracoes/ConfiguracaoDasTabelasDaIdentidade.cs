using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Renomeia as tabelas do store do IdentityCore, que por padrão nascem com o
/// prefixo "AspNet" em PascalCase (AspNetRoles, AspNetUserClaims, …), para a
/// convenção do schema: plural em snake_case, sem prefixo de framework. A tabela
/// de usuários (<c>usuarios</c>) é mapeada junto da entidade em
/// <see cref="ConfiguracaoDeUsuarioDaOrganizacao"/>; aqui ficam as tabelas cujos
/// tipos não temos como estender (papel, claims, logins e tokens).
/// </summary>
internal static class ConfiguracaoDasTabelasDaIdentidade
{
    public static void Aplicar(ModelBuilder construtor)
    {
        construtor.Entity<IdentityRole<Guid>>().ToTable("roles");
        construtor.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        construtor.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        construtor.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        construtor.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        construtor.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
    }
}
