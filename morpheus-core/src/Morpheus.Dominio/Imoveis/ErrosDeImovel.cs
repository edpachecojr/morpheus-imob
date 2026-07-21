using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Catálogo dos erros de negócio do imóvel. Centralizar os códigos aqui evita
/// literais espalhados e dá ao consumidor um ponto único para reagir a cada falha.
/// </summary>
public static class ErrosDeImovel
{
    public static readonly Erro CodigoObrigatorio =
        new("Imovel.CodigoObrigatorio", "Código de referência não pode ser vazio.");

    public static readonly Erro EnderecoObrigatorio =
        new("Imovel.EnderecoObrigatorio", "Endereço não pode ser vazio.");
}
