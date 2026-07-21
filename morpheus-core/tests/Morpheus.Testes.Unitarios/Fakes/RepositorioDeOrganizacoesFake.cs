using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>Guarda as organizações adicionadas, para o teste inspecionar o que foi fundado.</summary>
public sealed class RepositorioDeOrganizacoesFake : IRepositorioDeOrganizacoes
{
    private readonly Dictionary<Guid, Organizacao> _porId = [];

    public List<Organizacao> Adicionadas { get; } = [];
    public List<Organizacao> Atualizadas { get; } = [];

    public Task AdicionarAsync(Organizacao organizacao, CancellationToken cancelamento)
    {
        Adicionadas.Add(organizacao);
        _porId[organizacao.Id] = organizacao;
        return Task.CompletedTask;
    }

    public Task<Organizacao?> ObterPorIdAsync(Guid id, CancellationToken cancelamento)
        => Task.FromResult(_porId.GetValueOrDefault(id));

    public Task AtualizarAsync(Organizacao organizacao, CancellationToken cancelamento)
    {
        Atualizadas.Add(organizacao);
        _porId[organizacao.Id] = organizacao;
        return Task.CompletedTask;
    }
}
