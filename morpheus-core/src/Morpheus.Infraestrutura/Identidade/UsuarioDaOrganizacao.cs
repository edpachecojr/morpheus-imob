using Microsoft.AspNetCore.Identity;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Usuário do painel persistido pelo IdentityCore, vinculado a uma organização.
/// Mora na infraestrutura porque carrega o acoplamento ao SDK do Identity — o
/// domínio conhece apenas <see cref="IPertenceOrganizacao"/> e o <see cref="PapelDoUsuario"/>.
/// Reusa a mesma regra de vínculo imutável das entidades de domínio.
/// </summary>
public sealed class UsuarioDaOrganizacao : IdentityUser<Guid>, IPertenceOrganizacao
{
    public Guid OrganizacaoId { get; private set; }
    public PapelDoUsuario Papel { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;

    public void AtribuirOrganizacao(Guid organizacaoId)
        => OrganizacaoId = RegraDeVinculoComOrganizacao.AtribuirImutavel(OrganizacaoId, organizacaoId);

    public void DefinirPapel(PapelDoUsuario papel) => Papel = papel;

    public void DefinirNomeCompleto(string nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException("Nome completo não pode ser vazio.", nameof(nomeCompleto));
        NomeCompleto = nomeCompleto.Trim();
    }
}
