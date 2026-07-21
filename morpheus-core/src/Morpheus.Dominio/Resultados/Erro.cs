namespace Morpheus.Dominio.Resultados;

/// <summary>
/// Erro nomeado do domínio: um <see cref="Codigo"/> estável para o consumidor
/// programar em cima e uma <see cref="Descricao"/> legível com o contexto da
/// falha. É o vocabulário único de erro do sistema — tanto o <see cref="Resultado"/>
/// quanto as exceções de domínio carregam um.
/// Exemplo: <c>new Erro("Imovel.CodigoObrigatorio", "Código de referência não pode ser vazio.")</c>.
/// </summary>
public sealed record Erro(string Codigo, string Descricao)
{
    /// <summary>Ausência de erro; o erro que acompanha todo resultado de sucesso.</summary>
    public static readonly Erro Nenhum = new(string.Empty, string.Empty);

    /// <summary>Um valor nulo foi fornecido onde se esperava um valor presente.</summary>
    public static readonly Erro ValorNulo = new("Geral.ValorNulo", "Um valor nulo foi fornecido.");
}
