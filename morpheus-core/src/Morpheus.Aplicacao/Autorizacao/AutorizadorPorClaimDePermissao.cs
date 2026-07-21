using System.Security.Claims;

namespace Morpheus.Aplicacao.Autorizacao;

/// <summary>
/// Decide pela claim de permissão que o papel do usuário carrega. Sem sessão
/// autenticada nada é concedido — o default nega, e negar aqui é o que sustenta
/// a promessa de que endpoint sem declaração não passa.
/// </summary>
public sealed class AutorizadorPorClaimDePermissao : IAutorizadorDeAcesso
{
    public bool Pode(ClaimsPrincipal usuario, string permissao)
    {
        if (usuario.Identity?.IsAuthenticated != true)
            return false;
        return usuario.HasClaim(ClaimDePermissao.Tipo, permissao);
    }
}
