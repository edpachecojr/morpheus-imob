using Morpheus.Aplicacao.Senhas;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Provedor de token de recuperação de mentira: emite um token previsível e
/// aceita apenas o último emitido, o que reproduz a regra de uso único sem
/// depender do Data Protection.
/// </summary>
public sealed class TokensDeRecuperacaoFake : ITokensDeRecuperacaoDeSenha
{
    private readonly Dictionary<Guid, string> _validos = [];

    public List<string> Emitidos { get; } = [];

    public Task<string> GerarAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var token = $"token-{Emitidos.Count}";
        _validos[usuarioId] = token;
        Emitidos.Add(token);
        return Task.FromResult(token);
    }

    public Task<Resultado> RedefinirAsync(
        Guid usuarioId, string token, string novaSenha, CancellationToken cancelamento)
    {
        if (!_validos.TryGetValue(usuarioId, out var esperado) || esperado != token)
            return Task.FromResult(Resultado.DeFalha(ErrosDeAutenticacao.TokenDeRecuperacaoInvalido));

        _validos.Remove(usuarioId);
        return Task.FromResult(Resultado.DeSucesso());
    }
}
