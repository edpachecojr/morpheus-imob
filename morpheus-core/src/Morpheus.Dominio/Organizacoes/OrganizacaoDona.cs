namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Identificador de uma <see cref="Organizacao"/> como value object: encapsula o
/// <see cref="Guid"/> e garante a invariante mínima de que o tenant nunca é vazio.
/// Toda entidade de negócio recebe um destes na construção — vínculo de tenant
/// válido por construção ("parse, don't validate"), sem carimbo posterior nem
/// barreira de isolamento na infraestrutura.
/// <para>
/// O tipo persistido/consultado permanece <see cref="Guid"/> (via
/// <see cref="Valor"/>): a chave primária de toda entidade é <see cref="Guid"/> em
/// <c>EntidadeBase</c>, e o EF Core exige que os dois lados de uma FK compartilhem
/// o mesmo tipo CLR — mapear a coluna como este VO quebraria a FK
/// <c>organizacao_id → organizacoes</c>. O VO vive na fronteira do domínio.
/// </para>
/// </summary>
public sealed record OrganizacaoDona
{
    public Guid Valor { get; }

    /// <summary>
    /// Cria o vínculo a partir de um id não vazio. Exemplo:
    /// <c>Imovel.Cadastrar(new OrganizacaoDona(contexto.OrganizacaoId), "AP-101", ...)</c>.
    /// </summary>
    public OrganizacaoDona(Guid valor)
    {
        if (valor == Guid.Empty)
            throw new ErroDeOrganizacaoObrigatoria(nameof(OrganizacaoDona));
        Valor = valor;
    }
}
