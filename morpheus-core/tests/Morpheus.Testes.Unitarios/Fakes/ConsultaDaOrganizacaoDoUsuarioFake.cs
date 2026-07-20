using Morpheus.Aplicacao.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Consulta falsa do vínculo usuário → organização, em memória. Conta as
/// chamadas para os testes provarem que o cache evita ir ao "banco".
/// </summary>
public sealed class ConsultaDaOrganizacaoDoUsuarioFake : IConsultaDaOrganizacaoDoUsuario
{
    private readonly Dictionary<Guid, Guid> _vinculos = new();

    public int QuantidadeDeChamadas { get; private set; }

    public void Vincular(Guid usuarioId, Guid organizacaoId) => _vinculos[usuarioId] = organizacaoId;

    public Task<Guid?> BuscarOrganizacaoIdAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        QuantidadeDeChamadas++;
        var encontrado = _vinculos.TryGetValue(usuarioId, out var organizacaoId);
        return Task.FromResult(encontrado ? organizacaoId : (Guid?)null);
    }
}
