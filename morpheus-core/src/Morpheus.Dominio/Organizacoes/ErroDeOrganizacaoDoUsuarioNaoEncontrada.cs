using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Usuário autenticado sem organização vinculada no repositório. Não deve
/// ocorrer em fluxo normal — todo usuário nasce ligado a uma organização — e
/// por isso falha alto em vez de assumir um tenant padrão.
/// </summary>
public sealed class ErroDeOrganizacaoDoUsuarioNaoEncontrada : ErroDeRegraDeNegocio
{
    public Guid UsuarioId { get; }

    public ErroDeOrganizacaoDoUsuarioNaoEncontrada(Guid usuarioId)
        : base($"Nenhuma organização vinculada ao usuário {usuarioId}.")
    {
        UsuarioId = usuarioId;
    }
}
