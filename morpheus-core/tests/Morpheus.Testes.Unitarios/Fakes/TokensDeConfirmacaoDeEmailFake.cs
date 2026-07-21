using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Provedor de token de confirmação de mentira: emite um token previsível e
/// aceita apenas o último emitido — reproduz a regra de "reenvio invalida o
/// anterior" sem depender do Data Protection.
/// </summary>
public sealed class TokensDeConfirmacaoDeEmailFake : ITokensDeConfirmacaoDeEmail
{
    private readonly Dictionary<Guid, string> _validos = [];

    public List<string> Emitidos { get; } = [];

    public Task<string> GerarAsync(Guid usuarioId, CancellationToken cancelamento)
        => Task.FromResult(Emitir(usuarioId));

    public Task<string> RenovarAsync(Guid usuarioId, CancellationToken cancelamento)
        => Task.FromResult(Emitir(usuarioId));

    public Task<Resultado> ConfirmarAsync(Guid usuarioId, string token, CancellationToken cancelamento)
    {
        if (!_validos.TryGetValue(usuarioId, out var esperado) || esperado != token)
            return Task.FromResult(Resultado.DeFalha(ErrosDeAutenticacao.TokenDeConfirmacaoInvalido));

        _validos.Remove(usuarioId);
        return Task.FromResult(Resultado.DeSucesso());
    }

    private string Emitir(Guid usuarioId)
    {
        var token = $"token-{Emitidos.Count}";
        _validos[usuarioId] = token;
        Emitidos.Add(token);
        return token;
    }
}
