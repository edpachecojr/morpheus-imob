using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Implementação de teste de <see cref="IContextoDoUsuario"/> que lê a identidade
/// de <see cref="IdentidadeDeTesteAtual"/> em vez do HttpContext. Substitui o
/// contexto HTTP na composição da app para os testes dirigirem o isolamento pelo
/// mesmo grafo de dependências que a produção usa.
/// </summary>
public sealed class ContextoDoUsuarioDeTeste : IContextoDoUsuario
{
    private readonly IdentidadeDeTesteAtual _identidade;

    public ContextoDoUsuarioDeTeste(IdentidadeDeTesteAtual identidade) => _identidade = identidade;

    public Guid? UsuarioAutenticadoId => _identidade.UsuarioId;
}
