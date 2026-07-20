namespace Morpheus.Testes.Integracao.Infraestrutura;

/// <summary>
/// Ocupa, na composição de teste, o lugar da sessão HTTP: um único ponto que diz
/// qual usuário está "autenticado" na execução corrente. Usa <see cref="AsyncLocal{T}"/>
/// para que a identidade acompanhe o fluxo assíncrono do teste sem vazar entre
/// execuções concorrentes.
/// </summary>
public sealed class IdentidadeDeTesteAtual
{
    private static readonly AsyncLocal<Guid?> UsuarioCorrente = new();

    public Guid? UsuarioId => UsuarioCorrente.Value;

    public void Autenticar(Guid usuarioId) => UsuarioCorrente.Value = usuarioId;

    public void Encerrar() => UsuarioCorrente.Value = null;
}
