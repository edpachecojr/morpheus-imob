using Morpheus.Api.Autorizacao;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Usuarios;

/// <summary>
/// Equipe da organização do contexto. Exige <c>usuario.gerenciar</c> — a primeira
/// rota restrita do sistema, e por isso a que prova na prática que o corretor
/// recebe 403 onde o dono recebe 200 (E1-F3-H2).
/// </summary>
public sealed class EndpointDeUsuariosDaOrganizacao : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas) =>
        rotas.MapGet("/organizacao/usuarios", Listar)
             .RequerPermissao(PermissoesDoPainel.UsuarioGerenciar)
             .WithName("ListarUsuariosDaOrganizacao");

    private static async Task<IResult> Listar(
        IDiretorioDeUsuarios usuarios, CancellationToken cancelamento) =>
        Results.Ok(await usuarios.ListarDaOrganizacaoAsync(cancelamento));
}
