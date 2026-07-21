using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Tentativa de resolver a organização do contexto sem usuário autenticado. O
/// default é seguro, não permissivo: sem identidade, o acesso a dados é negado
/// em vez de recair sobre alguma organização padrão.
/// </summary>
public sealed class ErroDeUsuarioNaoAutenticado : ErroDeRegraDeNegocio
{
    public ErroDeUsuarioNaoAutenticado() : base(ErrosDeUsuario.NaoAutenticado)
    {
    }
}
