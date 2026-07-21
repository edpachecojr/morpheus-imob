using System.Security.Claims;
using Morpheus.Aplicacao.Autorizacao;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Autorizacao;

/// <summary>
/// Prova o ponto único de decisão: concede só com a claim exata, e nega tudo sem
/// sessão — inclusive quando a claim está lá, o que aconteceria se alguém
/// montasse um principal à mão sem autenticar.
/// </summary>
public sealed class AutorizadorPorClaimDePermissaoTestes
{
    private readonly AutorizadorPorClaimDePermissao _autorizador = new();

    [Fact]
    public void Concede_quando_o_papel_carrega_a_permissao()
    {
        var usuario = Autenticado(PermissoesDoPainel.UsuarioGerenciar);

        Assert.True(_autorizador.Pode(usuario, PermissoesDoPainel.UsuarioGerenciar));
    }

    [Fact]
    public void Nega_quando_a_permissao_nao_esta_entre_as_claims()
    {
        var usuario = Autenticado(PermissoesDoPainel.ImovelLer);

        Assert.False(_autorizador.Pode(usuario, PermissoesDoPainel.FaturamentoGerenciar));
    }

    [Fact]
    public void Nega_sem_sessao_mesmo_com_a_claim_presente()
    {
        var identidade = new ClaimsIdentity([new Claim(ClaimDePermissao.Tipo, PermissoesDoPainel.ImovelLer)]);

        Assert.False(_autorizador.Pode(new ClaimsPrincipal(identidade), PermissoesDoPainel.ImovelLer));
    }

    [Fact]
    public void Nega_principal_anonimo()
        => Assert.False(_autorizador.Pode(new ClaimsPrincipal(), PermissoesDoPainel.ImovelLer));

    // Identidade com tipo de autenticação definido: é o que a torna autenticada.
    private static ClaimsPrincipal Autenticado(params string[] permissoes)
    {
        var claims = permissoes.Select(permissao => new Claim(ClaimDePermissao.Tipo, permissao));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "teste"));
    }
}
