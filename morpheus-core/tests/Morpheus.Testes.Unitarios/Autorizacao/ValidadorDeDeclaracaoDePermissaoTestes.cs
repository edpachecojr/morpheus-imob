using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Morpheus.Api.Autorizacao;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Autorizacao;

/// <summary>
/// Prova que "negar por padrão" é estrutural: rota que não declarou nada é
/// apontada pelo nome, e a subida falha antes de a rota atender alguém.
/// </summary>
public sealed class ValidadorDeDeclaracaoDePermissaoTestes
{
    [Fact]
    public void Rota_sem_declaracao_e_apontada_pelo_nome()
    {
        var rotas = new[] { Rota("GET /esquecida") };

        Assert.Equal(["GET /esquecida"], ValidadorDeDeclaracaoDePermissao.RotasSemDeclaracao(rotas));
    }

    [Fact]
    public void Rota_com_permissao_declarada_passa()
    {
        var rotas = new[] { Rota("GET /imoveis", new PermissaoExigida(PermissoesDoPainel.ImovelLer)) };

        Assert.Empty(ValidadorDeDeclaracaoDePermissao.RotasSemDeclaracao(rotas));
    }

    [Fact]
    public void Rota_que_exige_apenas_sessao_passa()
    {
        var rotas = new[] { Rota("DELETE /sessoes/atual", new ApenasSessaoExigida()) };

        Assert.Empty(ValidadorDeDeclaracaoDePermissao.RotasSemDeclaracao(rotas));
    }

    [Fact]
    public void Rota_anonima_explicita_passa()
    {
        var rotas = new[] { Rota("GET /health", new AllowAnonymousAttribute()) };

        Assert.Empty(ValidadorDeDeclaracaoDePermissao.RotasSemDeclaracao(rotas));
    }

    [Fact]
    public void Falha_alto_citando_todas_as_rotas_pendentes()
    {
        var rotas = new[] { Rota("GET /a"), Rota("POST /b"), Rota("GET /c", new ApenasSessaoExigida()) };

        var erro = Assert.Throws<InvalidOperationException>(
            () => ValidadorDeDeclaracaoDePermissao.GarantirQueTodasDeclaram(rotas));

        Assert.Contains("GET /a", erro.Message, StringComparison.Ordinal);
        Assert.Contains("POST /b", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("GET /c", erro.Message, StringComparison.Ordinal);
    }

    private static Endpoint Rota(string nome, params object[] metadados)
        => new(_ => Task.CompletedTask, new EndpointMetadataCollection(metadados), nome);
}
