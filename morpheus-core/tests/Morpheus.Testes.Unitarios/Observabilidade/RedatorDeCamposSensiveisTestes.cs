using Morpheus.Api.Observabilidade;
using Morpheus.Testes.Unitarios.Fakes;
using Serilog;

namespace Morpheus.Testes.Unitarios.Observabilidade;

/// <summary>
/// Prova o critério anti-vazamento (E1-F4-H1): propriedade de log com nome de
/// segredo ou dado pessoal sai mascarada; propriedade comum passa intacta.
/// </summary>
public sealed class RedatorDeCamposSensiveisTestes
{
    [Theory]
    [InlineData("Senha")]
    [InlineData("senha_do_usuario")]
    [InlineData("Password")]
    [InlineData("AccessToken")]
    [InlineData("Authorization")]
    [InlineData("ApiKey")]
    [InlineData("SenhaHash")]
    [InlineData("Cpf")]
    [InlineData("cnpj")]
    public void Mascara_propriedade_com_nome_sensivel(string propriedade)
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);

        diario.Information("evento com {" + propriedade + "}", "valor-cru-secreto");

        Assert.Equal(RedatorDeCamposSensiveis.Mascara, coletor.ValorDe(propriedade));
    }

    [Fact]
    public void Nao_altera_propriedade_comum()
    {
        var coletor = new ColetorDeEventosDeLog();
        var diario = ConstruirDiario(coletor);

        diario.Information("imóvel {Endereco}", "Rua das Flores, 100");

        Assert.Equal("Rua das Flores, 100", coletor.ValorDe("Endereco"));
    }

    private static ILogger ConstruirDiario(ColetorDeEventosDeLog coletor) =>
        new LoggerConfiguration()
            .Enrich.With(new RedatorDeCamposSensiveis())
            .WriteTo.Sink(coletor)
            .CreateLogger();
}
