namespace Morpheus.Dominio.Resultados;

/// <summary>
/// Desfecho de uma operação sem exceção: ou sucesso, ou falha portando um
/// <see cref="Erro"/>. É como as camadas comunicam falhas esperadas (entrada
/// inválida, recurso ausente) — exceção fica reservada para erro de fato.
/// Exemplo: <c>return Resultado.DeFalha(ErrosDeImovel.CodigoObrigatorio);</c>.
/// </summary>
public class Resultado
{
    protected internal Resultado(bool sucesso, Erro erro)
    {
        if (sucesso && erro != Erro.Nenhum)
            throw new InvalidOperationException(
                $"Resultado de sucesso não pode portar erro; recebido '{erro.Codigo}'.");
        if (!sucesso && erro == Erro.Nenhum)
            throw new InvalidOperationException("Resultado de falha exige um erro; recebido Erro.Nenhum.");

        Sucesso = sucesso;
        Erro = erro;
    }

    public bool Sucesso { get; }

    public bool Falha => !Sucesso;

    public Erro Erro { get; }

    public static Resultado DeSucesso() => new(true, Erro.Nenhum);

    public static Resultado DeFalha(Erro erro) => new(false, erro);

    public static Resultado<TValor> DeSucesso<TValor>(TValor valor) => new(valor, true, Erro.Nenhum);

    public static Resultado<TValor> DeFalha<TValor>(Erro erro) => new(default, false, erro);
}
