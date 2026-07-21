namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Conferência de senha e contagem de falhas por conta — o "rate limit por conta"
/// exigido em [autenticacao.md](../../../../docs/fundacao/autenticacao.md). Fina
/// de propósito: o caso de uso de login orquestra a sequência, sem conhecer o
/// hasher nem a política de bloqueio do Identity.
/// </summary>
public interface IVerificadorDeSenha
{
    /// <summary>Confere a senha do usuário. Não altera contagem de falhas nem abre sessão.</summary>
    Task<bool> ConferirAsync(Guid usuarioId, string senha, CancellationToken cancelamento);

    /// <summary>
    /// Gasta o mesmo tempo de uma conferência real sem que exista usuário para
    /// conferir. Sem isso, um e-mail inexistente responde perceptivelmente mais
    /// rápido que uma senha errada e o tempo de resposta vira oráculo de
    /// enumeração de contas.
    /// </summary>
    Task ConsumirTempoEquivalenteAsync(CancellationToken cancelamento);

    /// <summary>Registra uma falha, bloqueando a conta ao ultrapassar o limite de tentativas.</summary>
    Task RegistrarFalhaAsync(Guid usuarioId, CancellationToken cancelamento);

    /// <summary>Zera a contagem de falhas após um login bem-sucedido.</summary>
    Task LimparFalhasAsync(Guid usuarioId, CancellationToken cancelamento);
}
