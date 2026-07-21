using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Catálogo dos erros de isolamento multi-tenant. Descrevem violações das
/// barreiras entre organizações — carregam os ids envolvidos para que a falha
/// diga exatamente qual escrita/leitura foi recusada e por quê (ADR-0003).
/// </summary>
public static class ErrosDeIsolamento
{
    public static Erro OrganizacaoDivergente(Guid organizacaoDaEntidade, Guid organizacaoDoContexto) =>
        new("Isolamento.OrganizacaoDivergente",
            $"Escrita rejeitada: entidade da organização {organizacaoDaEntidade} não " +
            $"pertence à organização do contexto {organizacaoDoContexto}.");

    public static Erro VinculoImutavel(Guid vinculoAtual, Guid vinculoRecusado) =>
        new("Isolamento.VinculoImutavel",
            $"Vínculo de organização é imutável: entidade já pertence a {vinculoAtual} " +
            $"e não pode ser revinculada a {vinculoRecusado}.");

    public static Erro EscritaSemOrganizacao(string entidade) =>
        new("Isolamento.EscritaSemOrganizacao",
            $"Escrita rejeitada: entidade '{entidade}' não tem organização e não há " +
            "contexto de organização para carimbá-la.");

    public static Erro OrganizacaoObrigatoria(string operacao) =>
        new("Isolamento.OrganizacaoObrigatoria",
            $"Operação '{operacao}' exige um OrganizacaoId não vazio.");
}
