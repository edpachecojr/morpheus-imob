namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Para que o imóvel está anunciado. Determina a linguagem do resto do sistema
/// (ex.: um dossiê de locação pede documentos diferentes de um de venda).
/// </summary>
public enum FinalidadeDoImovel
{
    Locacao,
    Venda,
}
