using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>Guarda o último nome aplicado por usuário, para o teste conferir a atualização.</summary>
public sealed class AtualizacaoDeUsuarioFake : IAtualizacaoDeUsuario
{
    public Dictionary<Guid, string> NomesAtualizados { get; } = [];

    public Task<Resultado> AtualizarNomeCompletoAsync(
        Guid usuarioId, string nomeCompleto, CancellationToken cancelamento)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            return Task.FromResult(Resultado.DeFalha(ErrosDeUsuario.NomeCompletoObrigatorio));

        NomesAtualizados[usuarioId] = nomeCompleto;
        return Task.FromResult(Resultado.DeSucesso());
    }
}
