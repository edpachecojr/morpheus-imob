using Microsoft.EntityFrameworkCore;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Organizacoes;

/// <summary>
/// Escrita de organizações via EF Core. Grava na hora porque o usuário dono
/// referencia a organização por chave estrangeira e precisa encontrá-la; a
/// atomicidade dos dois vem da transação aberta pelo caso de uso, não de adiar a
/// gravação (E1-F1-H2).
/// </summary>
public sealed class RepositorioDeOrganizacoesComEfCore : IRepositorioDeOrganizacoes
{
    private readonly MorpheusDbContext _banco;

    public RepositorioDeOrganizacoesComEfCore(MorpheusDbContext banco) => _banco = banco;

    public async Task AdicionarAsync(Organizacao organizacao, CancellationToken cancelamento)
    {
        await _banco.Organizacoes.AddAsync(organizacao, cancelamento);
        await _banco.SaveChangesAsync(cancelamento);
    }

    public Task<Organizacao?> ObterPorIdAsync(Guid id, CancellationToken cancelamento)
        => _banco.Organizacoes.FirstOrDefaultAsync(organizacao => organizacao.Id == id, cancelamento);

    // A organização já chega rastreada por ObterPorIdAsync: persistir a alteração
    // é só confirmar a transação, sem um Update explícito redundante.
    public Task AtualizarAsync(Organizacao organizacao, CancellationToken cancelamento)
        => _banco.SaveChangesAsync(cancelamento);
}
