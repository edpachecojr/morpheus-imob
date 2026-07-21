using Morpheus.Api.Autorizacao;
using Morpheus.Api.Endpoints.Contas;
using Morpheus.Api.Endpoints.Emails;
using Morpheus.Api.Endpoints.Imoveis;
using Morpheus.Api.Endpoints.Organizacoes;
using Morpheus.Api.Endpoints.Saude;
using Morpheus.Api.Endpoints.Senhas;
using Morpheus.Api.Endpoints.Sessoes;
using Morpheus.Api.Endpoints.Usuarios;

namespace Morpheus.Api.Endpoints;

/// <summary>
/// Lista, num lugar só, todos os grupos de rota da aplicação e os mapeia. A lista
/// é escrita à mão de propósito: varredura de assembly acharia endpoint que
/// ninguém pretendia publicar, e esconderia o inventário de rotas justamente de
/// quem revisa segurança.
/// </summary>
public static class MapeamentoDeEndpoints
{
    private static readonly IEndpoint[] Grupos =
    [
        new EndpointDeSaude(),
        new EndpointDeCadastroDeConta(),
        new EndpointDeSessao(),
        new EndpointDeSenha(),
        new EndpointDeConfirmacaoDeEmail(),
        new EndpointDeImoveis(),
        new EndpointDeOrganizacao(),
        new EndpointDeUsuariosDaOrganizacao(),
    ];

    /// <summary>
    /// Mapeia todos os grupos e prova, na sequência, que nenhuma rota ficou sem
    /// declaração de acesso — a subida falha antes de atender a primeira requisição.
    /// </summary>
    public static WebApplication MapearEndpoints(this WebApplication aplicacao)
    {
        foreach (var grupo in Grupos)
            grupo.Mapear(aplicacao);

        ValidadorDeDeclaracaoDePermissao.GarantirQueTodasDeclaram(
            ((IEndpointRouteBuilder)aplicacao).DataSources.SelectMany(fonte => fonte.Endpoints));

        return aplicacao;
    }
}
