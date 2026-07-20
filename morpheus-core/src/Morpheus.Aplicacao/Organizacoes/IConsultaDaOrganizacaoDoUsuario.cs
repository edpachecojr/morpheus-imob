namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Busca no repositório o id da organização de um usuário. É a fonte da verdade
/// por trás do cache do resolvedor — chamada apenas quando o cache não tem o dado.
/// </summary>
public interface IConsultaDaOrganizacaoDoUsuario
{
    Task<Guid?> BuscarOrganizacaoIdAsync(Guid usuarioId, CancellationToken cancelamento);
}
