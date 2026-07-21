using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Escrita dos próprios dados do usuário do painel. Interface fina sobre o store
/// do Identity, separada de <see cref="IDiretorioDeUsuarios"/> porque muda por
/// outra razão: leitura e escrita têm ritmos de evolução diferentes.
/// </summary>
public interface IAtualizacaoDeUsuario
{
    /// <summary>
    /// Substitui o nome completo do usuário — a parte de "completar meus dados"
    /// do onboarding (E1-F1-H4) que não depende de nenhum campo novo.
    /// Exemplo: <c>await atualizacao.AtualizarNomeCompletoAsync(usuarioId, "Ana Souza", cancelamento)</c>.
    /// </summary>
    Task<Resultado> AtualizarNomeCompletoAsync(Guid usuarioId, string nomeCompleto, CancellationToken cancelamento);
}
