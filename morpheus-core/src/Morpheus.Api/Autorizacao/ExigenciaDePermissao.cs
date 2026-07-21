using Microsoft.AspNetCore.Authorization;

namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Exigência de uma permissão nomeada numa política de autorização. Traduz a
/// declaração da rota para o vocabulário do ASP.NET Core; quem decide continua
/// sendo o ponto único <c>IAutorizadorDeAcesso</c>.
/// </summary>
public sealed class ExigenciaDePermissao : IAuthorizationRequirement
{
    public ExigenciaDePermissao(string permissao) => Permissao = permissao;

    public string Permissao { get; }
}
