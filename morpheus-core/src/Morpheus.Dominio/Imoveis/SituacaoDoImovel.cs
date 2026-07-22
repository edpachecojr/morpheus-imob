namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Estado comercial do imóvel no catálogo. Nasce sempre <see cref="Disponivel"/>
/// (E2-F1-H1); as transições entre estados são regra da E2-F1-H3, não daqui.
/// </summary>
public enum SituacaoDoImovel
{
    Disponivel,
    Reservado,
    Indisponivel,
}
