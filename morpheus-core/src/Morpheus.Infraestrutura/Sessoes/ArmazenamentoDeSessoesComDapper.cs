using Dapper;
using Morpheus.Aplicacao.Sessoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Sessoes;

/// <summary>
/// Guarda as sessões do painel no Postgres, em conexão própria via Dapper. Não usa
/// o <see cref="MorpheusDbContext"/> pelo mesmo motivo da resolução de tenant: a
/// sessão é restaurada antes de existir escopo de requisição resolvido, e amarrá-la
/// ao contexto de dados que ela mesma habilita criaria dependência circular.
/// <para>
/// Sessão expirada não é apagada aqui — a consulta simplesmente não a encontra. A
/// varredura das linhas mortas entra com o agendador (E1-F0-H4); até lá elas só
/// ocupam espaço, nunca concedem acesso.
/// </para>
/// </summary>
public sealed class ArmazenamentoDeSessoesComDapper : IArmazenamentoDeSessoes
{
    private readonly IFabricaDeConexao _fabrica;
    private readonly TimeProvider _relogio;

    public ArmazenamentoDeSessoesComDapper(IFabricaDeConexao fabrica, TimeProvider relogio)
    {
        _fabrica = fabrica;
        _relogio = relogio;
    }

    public async Task GuardarAsync(SessaoPersistida sessao, CancellationToken cancelamento)
    {
        using var conexao = await _fabrica.AbrirAsync(cancelamento);
        await conexao.ExecuteAsync(new CommandDefinition(
            ComandosDeSessao.Guardar,
            new
            {
                sessao.Id,
                sessao.UsuarioId,
                sessao.Conteudo,
                sessao.ExpiraEm,
                CriadaEm = _relogio.GetUtcNow(),
            },
            cancellationToken: cancelamento));
    }

    public async Task RenovarAsync(SessaoPersistida sessao, CancellationToken cancelamento)
    {
        using var conexao = await _fabrica.AbrirAsync(cancelamento);
        await conexao.ExecuteAsync(new CommandDefinition(
            ComandosDeSessao.Renovar,
            new { sessao.Id, sessao.Conteudo, sessao.ExpiraEm },
            cancellationToken: cancelamento));
    }

    public async Task<SessaoPersistida?> BuscarAsync(Guid sessaoId, CancellationToken cancelamento)
    {
        using var conexao = await _fabrica.AbrirAsync(cancelamento);
        var linha = await conexao.QuerySingleOrDefaultAsync<LinhaDeSessao>(new CommandDefinition(
            ComandosDeSessao.Buscar,
            new { Id = sessaoId, Agora = _relogio.GetUtcNow() },
            cancellationToken: cancelamento));

        return linha?.ParaSessao();
    }

    public async Task RemoverAsync(Guid sessaoId, CancellationToken cancelamento)
    {
        using var conexao = await _fabrica.AbrirAsync(cancelamento);
        await conexao.ExecuteAsync(new CommandDefinition(
            ComandosDeSessao.Remover, new { Id = sessaoId }, cancellationToken: cancelamento));
    }

    public async Task RemoverDoUsuarioAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        using var conexao = await _fabrica.AbrirAsync(cancelamento);
        await conexao.ExecuteAsync(new CommandDefinition(
            ComandosDeSessao.RemoverDoUsuario, new { UsuarioId = usuarioId }, cancellationToken: cancelamento));
    }
}
