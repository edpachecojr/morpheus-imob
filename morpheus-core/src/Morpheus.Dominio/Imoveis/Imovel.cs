using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Dominio.Imoveis;

/// <summary>
/// Unidade anunciada por uma organização. O código de referência é único
/// dentro da organização — dois tenants podem usar "AP-101" sem conflito (E1-F1-H3).
/// Modelo mínimo da fundação; finalidade e situação entram no épico de ingestão.
/// Exemplo: <c>new Imovel("AP-101", "Rua das Acácias, 100", TimeProvider.System)</c>.
/// </summary>
public sealed class Imovel : EntidadeDaOrganizacao
{
    public Guid Id { get; private set; }
    public string CodigoDeReferencia { get; private set; }
    public string Endereco { get; private set; }
    public DateTimeOffset CadastradoEm { get; private set; }

    public Imovel(string codigoDeReferencia, string endereco, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(codigoDeReferencia))
            throw new ArgumentException("Código de referência não pode ser vazio.", nameof(codigoDeReferencia));
        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("Endereço não pode ser vazio.", nameof(endereco));

        Id = Guid.NewGuid();
        CodigoDeReferencia = codigoDeReferencia.Trim();
        Endereco = endereco.Trim();
        CadastradoEm = relogio.GetUtcNow();
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Imovel()
    {
        CodigoDeReferencia = string.Empty;
        Endereco = string.Empty;
    }
}
