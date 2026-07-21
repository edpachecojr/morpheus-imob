namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Nomes canônicos dos papéis do painel. Substituem o antigo enum de papel: quem
/// guarda o vínculo usuário → papel é o Identity (tabelas <c>roles</c> e
/// <c>user_roles</c>), então o domínio precisa apenas do nome estável que
/// atravessa banco, claim e teste sem tradução (ADR-0010).
/// <para>
/// Minúsculas e sem acento de propósito: o nome viaja em claim e em SQL, onde
/// maiúscula e acento só criam variação para errar.
/// </para>
/// </summary>
public static class PapeisDoUsuario
{
    /// <summary>Contratou o SaaS. Pode tudo dentro da organização, inclusive faturamento e usuários.</summary>
    public const string Dono = "dono";

    /// <summary>Atende lead, faz visita, opera dossiê. Não gerencia usuários nem faturamento.</summary>
    public const string Corretor = "corretor";

    /// <summary>
    /// Papéis existentes no MVP. Gestor e secretária entram na Fase 5 acrescentando
    /// linhas aqui e na <see cref="MatrizDePermissoes"/>, sem migração de dados.
    /// </summary>
    public static readonly IReadOnlyList<string> DoMvp = [Dono, Corretor];
}
