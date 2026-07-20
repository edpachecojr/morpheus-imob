namespace Morpheus.Aplicacao.Imoveis;

/// <summary>
/// Projeção de leitura de imóvel usada pelas consultas performáticas (Dapper).
/// Modelo de leitura enxuto, separado da entidade de domínio: a escrita passa
/// pelo <see cref="Morpheus.Dominio.Imoveis.Imovel"/>, a leitura rápida por aqui.
/// </summary>
public sealed record ImovelResumo(Guid Id, string CodigoDeReferencia, string Endereco);
