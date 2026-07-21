using System.Diagnostics.CodeAnalysis;

namespace Morpheus.Dominio.Resultados;

/// <summary>
/// Resultado que, em caso de sucesso, transporta um valor. Acessar
/// <see cref="Valor"/> numa falha é erro de programação e falha alto. As
/// conversões implícitas deixam o corpo das factories enxuto: devolver o valor
/// vira sucesso e devolver um <see cref="Erro"/> vira falha, sem cerimônia.
/// </summary>
public class Resultado<TValor> : Resultado
{
    private readonly TValor? _valor;

    protected internal Resultado(TValor? valor, bool sucesso, Erro erro)
        : base(sucesso, erro)
        => _valor = valor;

    [NotNull]
    public TValor Valor => Sucesso
        ? _valor!
        : throw new InvalidOperationException("Não se acessa o valor de um resultado de falha.");

    public static implicit operator Resultado<TValor>(TValor valor) => DeSucesso(valor);

    public static implicit operator Resultado<TValor>(Erro erro) => DeFalha<TValor>(erro);
}
