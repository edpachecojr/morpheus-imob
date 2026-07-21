using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Catálogo dos erros de isolamento multi-tenant. Descrevem violações das
/// barreiras entre organizações — carregam os ids envolvidos para que a falha
/// diga exatamente qual escrita/leitura foi recusada e por quê (ADR-0003).
/// </summary>
public static class ErrosDeIsolamento
{
    public static Erro VinculoImutavel(Guid vinculoAtual, Guid vinculoRecusado) =>
        new("Isolamento.VinculoImutavel",
            $"Vínculo de organização é imutável: entidade já pertence a {vinculoAtual} " +
            $"e não pode ser revinculada a {vinculoRecusado}.");

    public static Erro OrganizacaoObrigatoria(string operacao) =>
        new("Isolamento.OrganizacaoObrigatoria",
            $"Operação '{operacao}' exige um OrganizacaoId não vazio.");
}
