namespace Morpheus.Aplicacao.Autorizacao;

/// <summary>
/// Nome do tipo de claim que transporta uma permissão nomeada. As permissões são
/// claims do <b>papel</b> (tabela <c>role_claims</c>, semeada por migração), não do
/// usuário: quem muda de papel muda de permissões sem nenhuma linha por usuário
/// para corrigir (ADR-0010).
/// </summary>
public static class ClaimDePermissao
{
    public const string Tipo = "permissao";
}
