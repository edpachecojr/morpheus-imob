using Morpheus.Dominio.Comum;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Unidade anunciada por uma organização. O código de referência é único
/// dentro da organização — dois tenants podem usar "AP-101" sem conflito (E1-F1-H3).
/// Nasce por <see cref="Cadastrar"/> sempre com <see cref="SituacaoDoImovel.Disponivel"/>;
/// volta do banco por <see cref="Rehidratar"/>.
/// </summary>
public sealed class Imovel : EntidadeDaOrganizacao
{
    public string CodigoDeReferencia { get; private set; }
    public string Titulo { get; private set; }
    public FinalidadeDoImovel Finalidade { get; private set; }
    public SituacaoDoImovel Situacao { get; private set; }
    public string Endereco { get; private set; }

    /// <summary>Instante do cadastro — a linguagem do imóvel para <see cref="DadosDeAuditoria.CriadoEm"/>.</summary>
    public DateTimeOffset CadastradoEm => Auditoria.CriadoEm;

    private Imovel(
        Guid id,
        OrganizacaoDona organizacao,
        string codigoDeReferencia,
        string titulo,
        FinalidadeDoImovel finalidade,
        SituacaoDoImovel situacao,
        string endereco,
        DadosDeAuditoria auditoria)
        : base(organizacao)
    {
        Id = id;
        CodigoDeReferencia = codigoDeReferencia;
        Titulo = titulo;
        Finalidade = finalidade;
        Situacao = situacao;
        Endereco = endereco;
        Auditoria = auditoria;
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Imovel()
    {
        CodigoDeReferencia = string.Empty;
        Titulo = string.Empty;
        Endereco = string.Empty;
    }

    /// <summary>
    /// Cadastra um imóvel novo já vinculado à sua organização: recebe o tenant, gera
    /// identidade e auditoria, valida os campos e registra
    /// <see cref="ImovelCadastradoEvento"/> para o outbox drenar na escrita. A
    /// organização entra por <see cref="OrganizacaoDona"/> — quem cadastra é dono de
    /// defini-la (a partir do contexto autenticado), não a persistência. Entrada
    /// inválida é desfecho esperado, então vira <see cref="Resultado"/> — não exceção.
    /// Exemplo: <c>Imovel.Cadastrar(new OrganizacaoDona(contexto.OrganizacaoId), "AP-101",
    /// "Apartamento com vista para o parque", FinalidadeDoImovel.Locacao, "Rua das Acácias, 100", relogio)</c>.
    /// </summary>
    public static Resultado<Imovel> Cadastrar(
        OrganizacaoDona organizacao,
        string codigoDeReferencia,
        string titulo,
        FinalidadeDoImovel finalidade,
        string endereco,
        TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(codigoDeReferencia))
            return ErrosDeImovel.CodigoObrigatorio;
        if (string.IsNullOrWhiteSpace(titulo))
            return ErrosDeImovel.TituloObrigatorio;
        if (string.IsNullOrWhiteSpace(endereco))
            return ErrosDeImovel.EnderecoObrigatorio;

        var imovel = new Imovel(
            Guid.NewGuid(),
            organizacao,
            codigoDeReferencia.Trim(),
            titulo.Trim(),
            finalidade,
            SituacaoDoImovel.Disponivel,
            endereco.Trim(),
            DadosDeAuditoria.Nascer(relogio));
        imovel.RegistrarEvento(new ImovelCadastradoEvento(
            imovel.Id,
            imovel.CodigoDeReferencia,
            imovel.Titulo,
            imovel.Finalidade,
            imovel.Situacao,
            imovel.Endereco,
            imovel.CadastradoEm));
        return imovel;
    }

    /// <summary>
    /// Reconstrói um imóvel já persistido a partir de dados confiáveis (projeção
    /// manual, Dapper): não gera identidade nem auditoria e não revalida — o banco
    /// já é a fonte de verdade, e nenhum evento é registrado.
    /// Exemplo: <c>Imovel.Rehidratar(id, org, "AP-101", "Apartamento com vista para o parque",
    /// FinalidadeDoImovel.Locacao, SituacaoDoImovel.Disponivel, "Rua X, 1", criadoEm, atualizadoEm)</c>.
    /// </summary>
    public static Imovel Rehidratar(
        Guid id,
        Guid organizacaoId,
        string codigoDeReferencia,
        string titulo,
        FinalidadeDoImovel finalidade,
        SituacaoDoImovel situacao,
        string endereco,
        DateTimeOffset criadoEm,
        DateTimeOffset atualizadoEm)
    {
        return new Imovel(
            id,
            new OrganizacaoDona(organizacaoId),
            codigoDeReferencia,
            titulo,
            finalidade,
            situacao,
            endereco,
            DadosDeAuditoria.Rehidratar(criadoEm, atualizadoEm));
    }
}
