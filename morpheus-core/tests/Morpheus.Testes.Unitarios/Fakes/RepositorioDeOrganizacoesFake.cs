using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>Guarda as organizações adicionadas, para o teste inspecionar o que foi fundado.</summary>
public sealed class RepositorioDeOrganizacoesFake : IRepositorioDeOrganizacoes
{
    public List<Organizacao> Adicionadas { get; } = [];

    public Task AdicionarAsync(Organizacao organizacao, CancellationToken cancelamento)
    {
        Adicionadas.Add(organizacao);
        return Task.CompletedTask;
    }
}
