using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Resolução de organização acionada com um UsuarioId vazio. A identidade vem
/// do contexto autenticado; um id vazio aqui denuncia sessão corrompida, não
/// fluxo normal — falha alto em vez de resolver um tenant arbitrário.
/// </summary>
public sealed class ErroDeUsuarioObrigatorio : ErroDeRegraDeNegocio
{
    public ErroDeUsuarioObrigatorio() : base(ErrosDeUsuario.IdentificadorObrigatorio)
    {
    }
}
