using System.Security.Claims;
using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Api.Identidade;

/// <summary>
/// Deriva o usuário autenticado da sessão HTTP corrente (claim NameIdentifier).
/// Sem sessão, não há usuário — e o contexto de organização falha adiante,
/// mantendo o default seguro em vez de recair sobre alguma organização padrão.
/// </summary>
public sealed class ContextoDoUsuarioHttp : IContextoDoUsuario
{
    private readonly IHttpContextAccessor _acessor;

    public ContextoDoUsuarioHttp(IHttpContextAccessor acessor) => _acessor = acessor;

    public Guid? UsuarioAutenticadoId
    {
        get
        {
            var identificador = _acessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(identificador, out var id) ? id : null;
        }
    }
}
