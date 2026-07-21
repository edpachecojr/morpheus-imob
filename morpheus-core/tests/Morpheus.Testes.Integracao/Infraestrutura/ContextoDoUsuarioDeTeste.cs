using Morpheus.Api.Identidade;
using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Implementação de teste de <see cref="IContextoDoUsuario"/>: usa a identidade
/// dirigida por <see cref="IdentidadeDeTesteAtual"/> quando o teste define uma, e
/// cai no contexto HTTP real quando não define.
/// <para>
/// A queda para o HTTP é o que permite que os testes de autenticação passem pelo
/// cookie de verdade, no mesmo host, enquanto os testes de isolamento continuam
/// dirigindo o usuário na mão, sem precisar logar.
/// </para>
/// </summary>
public sealed class ContextoDoUsuarioDeTeste : IContextoDoUsuario
{
    private readonly IdentidadeDeTesteAtual _identidade;
    private readonly ContextoDoUsuarioHttp _sessaoHttp;

    public ContextoDoUsuarioDeTeste(IdentidadeDeTesteAtual identidade, ContextoDoUsuarioHttp sessaoHttp)
    {
        _identidade = identidade;
        _sessaoHttp = sessaoHttp;
    }

    public Guid? UsuarioAutenticadoId => _identidade.UsuarioId ?? _sessaoHttp.UsuarioAutenticadoId;
}
