using Microsoft.EntityFrameworkCore;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Infraestrutura.Persistencia;

namespace Morpheus.Infraestrutura.Organizacoes;

/// <summary>
/// Busca o id da organização de um usuário direto no store do Identity. É a
/// fonte da verdade por trás do cache; roda só no cache miss.
/// </summary>
public sealed class ConsultaDaOrganizacaoDoUsuarioComEfCore : IConsultaDaOrganizacaoDoUsuario
{
    private readonly MorpheusDbContext _banco;

    public ConsultaDaOrganizacaoDoUsuarioComEfCore(MorpheusDbContext banco) => _banco = banco;

    public async Task<Guid?> BuscarOrganizacaoIdAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        return await _banco.Users
            .Where(usuario => usuario.Id == usuarioId)
            .Select(usuario => (Guid?)usuario.OrganizacaoId)
            .SingleOrDefaultAsync(cancelamento);
    }
}
