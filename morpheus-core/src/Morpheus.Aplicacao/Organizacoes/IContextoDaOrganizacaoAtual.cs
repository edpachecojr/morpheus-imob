namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Ponto único que dá a organização do contexto para o filtro de isolamento.
/// Toda leitura e escrita de dado de negócio parte daqui — um lugar para auditar.
/// </summary>
public interface IContextoDaOrganizacaoAtual
{
    /// <summary>
    /// Organização do usuário autenticado. Falha se não houver usuário — o
    /// default é seguro, nunca uma organização padrão.
    /// </summary>
    Task<Guid> ObterOrganizacaoIdAsync(CancellationToken cancelamento);

    /// <summary>
    /// Organização do contexto, ou <c>null</c> quando não há usuário autenticado.
    /// Usado pelo interceptor de escrita, que tolera bootstrap e jobs com vínculo
    /// explícito em vez de exigir sempre uma sessão.
    /// </summary>
    Task<Guid?> ObterOrganizacaoIdOuNuloAsync(CancellationToken cancelamento);
}
