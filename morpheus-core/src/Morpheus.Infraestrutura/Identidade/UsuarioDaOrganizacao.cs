using Microsoft.AspNetCore.Identity;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Usuário do painel persistido pelo IdentityCore, vinculado a uma organização.
/// Mora na infraestrutura porque carrega o acoplamento ao SDK do Identity — o
/// domínio conhece apenas <see cref="IPertenceOrganizacao"/> e o <see cref="PapelDoUsuario"/>.
/// Diferente das entidades de domínio, o SDK exige construção sem parâmetros, então
/// o vínculo é definido depois por <see cref="VincularAOrganizacao"/> — ainda
/// imutável e não vazio, garantido pelo <see cref="OrganizacaoDona"/>.
/// </summary>
public sealed class UsuarioDaOrganizacao : IdentityUser<Guid>, IPertenceOrganizacao
{
    public Guid OrganizacaoId { get; private set; }
    public PapelDoUsuario Papel { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;

    /// <summary>
    /// Vincula o usuário à sua organização (não vazia, garantida pelo VO). Imutável:
    /// tentar revincular a outra organização é recusado, para o usuário não migrar de
    /// tenant por engano. Exemplo: <c>usuario.VincularAOrganizacao(new OrganizacaoDona(orgId))</c>.
    /// </summary>
    public void VincularAOrganizacao(OrganizacaoDona organizacao)
    {
        if (OrganizacaoId != Guid.Empty && OrganizacaoId != organizacao.Valor)
            throw new ErroDeVinculoDeOrganizacaoImutavel(OrganizacaoId, organizacao.Valor);
        OrganizacaoId = organizacao.Valor;
    }

    public void DefinirPapel(PapelDoUsuario papel) => Papel = papel;

    public void DefinirNomeCompleto(string nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ErroDeNomeCompletoObrigatorio();
        NomeCompleto = nomeCompleto.Trim();
    }
}
