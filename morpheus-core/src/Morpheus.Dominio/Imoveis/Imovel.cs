using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Unidade anunciada por uma organização. O código de referência é único
/// dentro da organização — dois tenants podem usar "AP-101" sem conflito (E1-F1-H3).
/// Modelo mínimo da fundação; finalidade e situação entram no épico de ingestão.
/// Nasce por <see cref="Cadastrar"/>; volta do banco por <see cref="Rehidratar"/>.
/// </summary>
public sealed class Imovel : EntidadeDaOrganizacao
{
    public Guid Id { get; private set; }
    public string CodigoDeReferencia { get; private set; }
    public string Endereco { get; private set; }
    public DateTimeOffset CadastradoEm { get; private set; }

    private Imovel(Guid id, string codigoDeReferencia, string endereco, DateTimeOffset cadastradoEm)
    {
        Id = id;
        CodigoDeReferencia = codigoDeReferencia;
        Endereco = endereco;
        CadastradoEm = cadastradoEm;
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Imovel()
    {
        CodigoDeReferencia = string.Empty;
        Endereco = string.Empty;
    }

    /// <summary>
    /// Cadastra um imóvel novo: gera identidade e auditoria e valida os campos.
    /// Entrada inválida é desfecho esperado, então vira <see cref="Resultado"/> —
    /// não exceção. Exemplo: <c>Imovel.Cadastrar("AP-101", "Rua das Acácias, 100", relogio)</c>.
    /// </summary>
    public static Resultado<Imovel> Cadastrar(string codigoDeReferencia, string endereco, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(codigoDeReferencia))
            return ErrosDeImovel.CodigoObrigatorio;
        if (string.IsNullOrWhiteSpace(endereco))
            return ErrosDeImovel.EnderecoObrigatorio;

        return new Imovel(Guid.NewGuid(), codigoDeReferencia.Trim(), endereco.Trim(), relogio.GetUtcNow());
    }

    /// <summary>
    /// Reconstrói um imóvel já persistido a partir de dados confiáveis (projeção
    /// manual, Dapper): não gera identidade nem auditoria e não revalida — o banco
    /// já é a fonte de verdade. Exemplo: <c>Imovel.Rehidratar(id, org, "AP-101", "Rua X, 1", quando)</c>.
    /// </summary>
    public static Imovel Rehidratar(
        Guid id, Guid organizacaoId, string codigoDeReferencia, string endereco, DateTimeOffset cadastradoEm)
    {
        var imovel = new Imovel(id, codigoDeReferencia, endereco, cadastradoEm);
        imovel.AtribuirOrganizacao(organizacaoId);
        return imovel;
    }
}
