using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Catálogo dos erros de negócio da organização (o tenant). Separado dos erros
/// de isolamento: aqui moram as regras de criação; lá, as barreiras entre tenants.
/// </summary>
public static class ErrosDeOrganizacao
{
    public static readonly Erro NomeObrigatorio =
        new("Organizacao.NomeObrigatorio", "Nome da organização não pode ser vazio.");
}
