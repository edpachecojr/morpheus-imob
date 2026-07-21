using System.Security.Claims;

namespace Morpheus.Aplicacao.Autorizacao;

/// <summary>
/// O <b>único</b> ponto de decisão de permissão do sistema (ADR-0005). Não existe
/// segunda forma de perguntar: nem <c>if (papel == "dono")</c> no caso de uso, nem
/// verificação ad hoc no endpoint.
/// <para>
/// A camada de tenant vem antes e não passa por aqui — ela é estrutural, na
/// consulta ([multi-tenancy.md](../../../../docs/fundacao/multi-tenancy.md)). A
/// camada de vínculo ("corretor vê só os próprios registros") entra no pós-MVP e
/// acrescentará o recurso à assinatura; enquanto não existe, um parâmetro de
/// recurso ignorado só daria falsa sensação de já estar coberta.
/// </para>
/// </summary>
public interface IAutorizadorDeAcesso
{
    /// <summary>
    /// Se a identidade autenticada tem a permissão nomeada. Exemplo:
    /// <c>autorizador.Pode(usuario, PermissoesDoPainel.UsuarioGerenciar)</c>.
    /// </summary>
    bool Pode(ClaimsPrincipal usuario, string permissao);
}
