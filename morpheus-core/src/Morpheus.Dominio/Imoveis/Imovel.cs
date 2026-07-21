using Morpheus.Dominio.Comum;
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
    public string CodigoDeReferencia { get; private set; }
    public string Endereco { get; private set; }

    /// <summary>Instante do cadastro — a linguagem do imóvel para <see cref="DadosDeAuditoria.CriadoEm"/>.</summary>
    public DateTimeOffset CadastradoEm => Auditoria.CriadoEm;

    private Imovel(Guid id, string codigoDeReferencia, string endereco, DadosDeAuditoria auditoria)
    {
        Id = id;
        CodigoDeReferencia = codigoDeReferencia;
        Endereco = endereco;
        Auditoria = auditoria;
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Imovel()
    {
        CodigoDeReferencia = string.Empty;
        Endereco = string.Empty;
    }

    /// <summary>
    /// Cadastra um imóvel novo: gera identidade e auditoria, valida os campos e
    /// registra <see cref="ImovelCadastradoEvento"/> para o outbox drenar na escrita.
    /// Entrada inválida é desfecho esperado, então vira <see cref="Resultado"/> —
    /// não exceção. Exemplo: <c>Imovel.Cadastrar("AP-101", "Rua das Acácias, 100", relogio)</c>.
    /// </summary>
    public static Resultado<Imovel> Cadastrar(string codigoDeReferencia, string endereco, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(codigoDeReferencia))
            return ErrosDeImovel.CodigoObrigatorio;
        if (string.IsNullOrWhiteSpace(endereco))
            return ErrosDeImovel.EnderecoObrigatorio;

        var imovel = new Imovel(
            Guid.NewGuid(), codigoDeReferencia.Trim(), endereco.Trim(), DadosDeAuditoria.Nascer(relogio));
        imovel.RegistrarEvento(new ImovelCadastradoEvento(
            imovel.Id, imovel.CodigoDeReferencia, imovel.Endereco, imovel.CadastradoEm));
        return imovel;
    }

    /// <summary>
    /// Reconstrói um imóvel já persistido a partir de dados confiáveis (projeção
    /// manual, Dapper): não gera identidade nem auditoria e não revalida — o banco
    /// já é a fonte de verdade, e nenhum evento é registrado.
    /// Exemplo: <c>Imovel.Rehidratar(id, org, "AP-101", "Rua X, 1", criadoEm, atualizadoEm)</c>.
    /// </summary>
    public static Imovel Rehidratar(
        Guid id,
        Guid organizacaoId,
        string codigoDeReferencia,
        string endereco,
        DateTimeOffset criadoEm,
        DateTimeOffset atualizadoEm)
    {
        var imovel = new Imovel(
            id, codigoDeReferencia, endereco, DadosDeAuditoria.Rehidratar(criadoEm, atualizadoEm));
        imovel.AtribuirOrganizacao(organizacaoId);
        return imovel;
    }
}
