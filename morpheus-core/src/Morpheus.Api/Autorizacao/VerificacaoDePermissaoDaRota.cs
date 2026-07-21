using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Morpheus.Aplicacao.Autorizacao;

namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Liga a política de autorização do ASP.NET Core ao ponto único de decisão. Não
/// decide nada por conta própria: pergunta ao <see cref="IAutorizadorDeAcesso"/> e
/// registra a negação com usuário, permissão e recurso — acúmulo de negação é
/// sinal de bug de UX ou de tentativa de abuso
/// ([autorizacao.md](../../../../docs/fundacao/autorizacao.md), regra 5).
/// </summary>
public sealed class VerificacaoDePermissaoDaRota : AuthorizationHandler<ExigenciaDePermissao>
{
    private readonly IAutorizadorDeAcesso _autorizador;
    private readonly IHttpContextAccessor _acessor;
    private readonly ILogger<VerificacaoDePermissaoDaRota> _diario;

    public VerificacaoDePermissaoDaRota(
        IAutorizadorDeAcesso autorizador,
        IHttpContextAccessor acessor,
        ILogger<VerificacaoDePermissaoDaRota> diario)
    {
        _autorizador = autorizador;
        _acessor = acessor;
        _diario = diario;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext contexto, ExigenciaDePermissao exigencia)
    {
        if (_autorizador.Pode(contexto.User, exigencia.Permissao))
        {
            contexto.Succeed(exigencia);
            return Task.CompletedTask;
        }

        RegistrarNegacao(contexto.User, exigencia.Permissao);
        return Task.CompletedTask;
    }

    // Id do usuário, nunca e-mail nem nome: log não carrega dado pessoal do
    // cliente (CLAUDE.md §Logging).
    private void RegistrarNegacao(ClaimsPrincipal usuario, string permissao) =>
        _diario.LogWarning(
            "Acesso negado por permissão: {usuario_id} sem {permissao} em {recurso}",
            usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonimo",
            permissao,
            _acessor.HttpContext?.Request.Path.Value ?? "desconhecido");
}
