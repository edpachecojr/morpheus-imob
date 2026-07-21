namespace Morpheus.Dominio.Comum;

/// <summary>
/// Trilha de auditoria mínima de uma entidade — quando nasceu e quando mudou pela
/// última vez. Value object imutável: cada mudança devolve uma nova instância, o
/// que impede corromper <see cref="CriadoEm"/> ao registrar uma alteração. Concentra
/// aqui o que antes cada entidade carimbava à mão, para não repetir a regra.
/// Exemplo: <c>Auditoria = DadosDeAuditoria.Nascer(relogio);</c>.
/// </summary>
public sealed record DadosDeAuditoria(DateTimeOffset CriadoEm, DateTimeOffset AtualizadoEm)
{
    /// <summary>
    /// Auditoria de uma entidade recém-criada: nascimento e última alteração são o
    /// mesmo instante, lido de um relógio injetado para manter o teste repetível.
    /// </summary>
    public static DadosDeAuditoria Nascer(TimeProvider relogio)
    {
        var agora = relogio.GetUtcNow();
        return new DadosDeAuditoria(agora, agora);
    }

    /// <summary>
    /// Nova auditoria após uma alteração: preserva <see cref="CriadoEm"/> e avança
    /// <see cref="AtualizadoEm"/> para o instante atual do relógio.
    /// </summary>
    public DadosDeAuditoria RegistrarAlteracao(TimeProvider relogio)
        => this with { AtualizadoEm = relogio.GetUtcNow() };

    /// <summary>
    /// Reconstrói a auditoria de dados já persistidos (Dapper, projeção manual):
    /// não gera instante novo, o banco é a fonte de verdade.
    /// </summary>
    public static DadosDeAuditoria Rehidratar(DateTimeOffset criadoEm, DateTimeOffset atualizadoEm)
        => new(criadoEm, atualizadoEm);
}
