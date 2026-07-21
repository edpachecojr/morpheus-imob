using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Erros;

/// <summary>
/// Falha de regra de negócio do domínio, reservada a erro de fato — não a
/// desfecho esperado, que vira <see cref="Resultado"/>. Carrega o mesmo
/// <see cref="Erro"/> (código + descrição) do vocabulário de falha, para que o
/// tratamento de erro traduza exceção e resultado pela mesma via.
/// </summary>
public abstract class ErroDeRegraDeNegocio : Exception
{
    public Erro Erro { get; }

    protected ErroDeRegraDeNegocio(Erro erro) : base(erro.Descricao) => Erro = erro;
}
