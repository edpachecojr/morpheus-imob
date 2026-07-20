using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Contexto de usuário falso: permite simular requisição autenticada ou sem
/// sessão, sem depender do HttpContext.
/// </summary>
public sealed class ContextoDoUsuarioFake : IContextoDoUsuario
{
    public Guid? UsuarioAutenticadoId { get; private set; }

    public static ContextoDoUsuarioFake Autenticado(Guid usuarioId) => new() { UsuarioAutenticadoId = usuarioId };

    public static ContextoDoUsuarioFake SemSessao() => new() { UsuarioAutenticadoId = null };
}
