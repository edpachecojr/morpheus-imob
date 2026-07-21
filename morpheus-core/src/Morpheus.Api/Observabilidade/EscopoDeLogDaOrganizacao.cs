using Morpheus.Aplicacao.Organizacoes;
using Serilog.Context;

namespace Morpheus.Api.Observabilidade;

/// <summary>
/// Empurra o id da organização do contexto para o <see cref="LogContext"/>, de
/// modo que toda linha de log da requisição carregue <c>organizacao_id</c> — o
/// tenant_id do sistema (E1-F4-H1). Sem sessão autenticada, nada é adicionado: o
/// default é seguro e o log simplesmente não traz o campo.
///
/// Deve rodar depois da autenticação (para haver usuário a resolver) e antes do
/// registro de requisições, para que o resumo da requisição também carregue o id.
/// </summary>
public sealed class EscopoDeLogDaOrganizacao
{
    public const string CampoOrganizacao = "organizacao_id";

    private readonly RequestDelegate _proximo;

    public EscopoDeLogDaOrganizacao(RequestDelegate proximo) => _proximo = proximo;

    public async Task InvokeAsync(HttpContext contexto, IContextoDaOrganizacaoAtual organizacao)
    {
        var organizacaoId = await organizacao.ObterOrganizacaoIdOuNuloAsync(contexto.RequestAborted);
        if (organizacaoId is null)
        {
            await _proximo(contexto);
            return;
        }

        using (LogContext.PushProperty(CampoOrganizacao, organizacaoId.Value))
            await _proximo(contexto);
    }
}
