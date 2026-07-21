using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Morpheus.Aplicacao.Autorizacao;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Semeia os papéis do MVP e suas permissões como dado versionado: cada linha da
/// <see cref="MatrizDePermissoes"/> vira uma claim do papel em <c>role_claims</c>,
/// gerada por migração (ADR-0010). Semear por migração, e não por rotina de
/// subida, mantém a matriz revisável no diff e idêntica em todo ambiente.
/// <para>
/// Ids fixos, aqui e não gerados: <c>HasData</c> exige valor determinístico, senão
/// cada <c>migrations add</c> produziria um apagar-e-inserir da tabela inteira.
/// Por isso a lista de permissões de um papel é <b>append-only</b> — inserir no
/// meio renumera as claims seguintes e gera uma migração maior que o necessário.
/// </para>
/// </summary>
internal static class ConfiguracaoDePapeisEPermissoes
{
    private static readonly Dictionary<string, Guid> IdsDosPapeis = new(StringComparer.Ordinal)
    {
        [PapeisDoUsuario.Dono] = Guid.Parse("9f1b7a10-0000-4000-8000-000000000001"),
        [PapeisDoUsuario.Corretor] = Guid.Parse("9f1b7a10-0000-4000-8000-000000000002"),
    };

    public static void Aplicar(ModelBuilder construtor)
    {
        construtor.Entity<IdentityRole<Guid>>().HasData(Papeis());
        construtor.Entity<IdentityRoleClaim<Guid>>().HasData(Permissoes());
    }

    private static IdentityRole<Guid>[] Papeis() =>
    [
        .. PapeisDoUsuario.DoMvp.Select(papel => new IdentityRole<Guid>
        {
            Id = IdsDosPapeis[papel],
            Name = papel,
            NormalizedName = papel.ToUpperInvariant(),
            // Carimbo fixo: o valor só serve para detectar escrita concorrente no
            // papel, e papel semeado não é editado em tempo de execução.
            ConcurrencyStamp = IdsDosPapeis[papel].ToString(),
        }),
    ];

    private static IdentityRoleClaim<Guid>[] Permissoes()
    {
        var claims = new List<IdentityRoleClaim<Guid>>();
        foreach (var papel in PapeisDoUsuario.DoMvp)
            foreach (var permissao in MatrizDePermissoes.Do(papel))
                claims.Add(new IdentityRoleClaim<Guid>
                {
                    Id = claims.Count + 1,
                    RoleId = IdsDosPapeis[papel],
                    ClaimType = ClaimDePermissao.Tipo,
                    ClaimValue = permissao,
                });

        return [.. claims];
    }
}
