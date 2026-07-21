using Morpheus.Api.Autorizacao;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Api.Endpoints.Usuarios;

/// <summary>
/// Equipe da organização do contexto. A listagem exige <c>usuario.gerenciar</c> —
/// a primeira rota restrita do sistema, e por isso a que prova na prática que o
/// corretor recebe 403 onde o dono recebe 200 (E1-F3-H2). Já editar os próprios
/// dados (E1-F1-H4) exige só sessão: qualquer papel edita o próprio nome.
/// </summary>
public sealed class EndpointDeUsuariosDaOrganizacao : IEndpoint
{
    public void Mapear(IEndpointRouteBuilder rotas)
    {
        rotas.MapGet("/organizacao/usuarios", Listar)
             .RequerPermissao(PermissoesDoPainel.UsuarioGerenciar)
             .WithName("ListarUsuariosDaOrganizacao");

        rotas.MapPatch("/organizacao/usuarios/atual", AtualizarMeusDados)
             .RequerApenasSessao()
             .WithName("AtualizarMeusDados");
    }

    private static async Task<IResult> Listar(
        IDiretorioDeUsuarios usuarios, CancellationToken cancelamento) =>
        Results.Ok(await usuarios.ListarDaOrganizacaoAsync(cancelamento));

    private static async Task<IResult> AtualizarMeusDados(
        RequisicaoDeAtualizacaoDeUsuario requisicao,
        AtualizacaoDeDadosDoUsuario atualizacao,
        CancellationToken cancelamento)
    {
        var resultado = await atualizacao.ExecutarAsync(requisicao.NomeCompleto, cancelamento);
        return resultado.Sucesso
            ? Results.NoContent()
            : RespostaDeFalha.De(resultado.Erro, StatusCodes.Status400BadRequest);
    }
}
