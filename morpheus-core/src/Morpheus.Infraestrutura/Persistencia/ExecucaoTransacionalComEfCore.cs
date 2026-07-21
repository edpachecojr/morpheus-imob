using Microsoft.EntityFrameworkCore;
using Morpheus.Aplicacao.Comum;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Infraestrutura.Persistencia;

/// <summary>
/// Transação explícita sobre o <see cref="MorpheusDbContext"/>. Abrange todas as
/// gravações feitas pelo contexto no bloco — inclusive as que o store do Identity
/// dispara por conta própria, já que compartilha esta mesma instância.
/// </summary>
public sealed class ExecucaoTransacionalComEfCore : IExecucaoTransacional
{
    private readonly MorpheusDbContext _banco;

    public ExecucaoTransacionalComEfCore(MorpheusDbContext banco) => _banco = banco;

    public async Task<Resultado> ExecutarAsync(
        Func<CancellationToken, Task<Resultado>> operacao, CancellationToken cancelamento)
    {
        // Já dentro de uma transação: quem a abriu é dono de confirmá-la, e abrir
        // outra aqui só criaria um ponto de commit parcial.
        if (_banco.Database.CurrentTransaction is not null)
            return await operacao(cancelamento);

        await using var transacao = await _banco.Database.BeginTransactionAsync(cancelamento);
        var resultado = await operacao(cancelamento);

        if (resultado.Falha)
        {
            await transacao.RollbackAsync(cancelamento);
            return resultado;
        }

        await transacao.CommitAsync(cancelamento);
        return resultado;
    }
}
