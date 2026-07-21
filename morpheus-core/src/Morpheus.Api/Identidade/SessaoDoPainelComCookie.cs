using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Morpheus.Aplicacao.Sessoes;
using Morpheus.Infraestrutura.Identidade;

namespace Morpheus.Api.Identidade;

/// <summary>
/// Abre e encerra a sessão do painel sobre o cookie do host. Monta a identidade
/// pelo <see cref="IUserClaimsPrincipalFactory{TUser}"/> do Identity — é ele que
/// traz os papéis do usuário e as permissões de cada papel para dentro da sessão.
/// <para>
/// Não usa <c>SignInManager</c> de propósito: ele embute fluxos que não temos (dois
/// fatores, login externo, confirmação) e esconderia qual conferência de senha
/// realmente aconteceu. Aqui a conferência é do caso de uso, e este tipo só emite
/// a sessão de quem já passou por ela.
/// </para>
/// </summary>
public sealed class SessaoDoPainelComCookie : ISessaoDoPainel
{
    private readonly IHttpContextAccessor _acessor;
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;
    private readonly IUserClaimsPrincipalFactory<UsuarioDaOrganizacao> _identidades;
    private readonly IArmazenamentoDeSessoes _sessoes;

    public SessaoDoPainelComCookie(
        IHttpContextAccessor acessor,
        UserManager<UsuarioDaOrganizacao> usuarios,
        IUserClaimsPrincipalFactory<UsuarioDaOrganizacao> identidades,
        IArmazenamentoDeSessoes sessoes)
    {
        _acessor = acessor;
        _usuarios = usuarios;
        _identidades = identidades;
        _sessoes = sessoes;
    }

    public async Task AbrirAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByIdAsync(usuarioId.ToString())
            ?? throw new InvalidOperationException(
                $"Sessão pedida para o usuário {usuarioId}, que não existe no registro.");

        var identidade = await _identidades.CreateAsync(usuario);
        await ContextoObrigatorio().SignInAsync(ConfiguracaoDeAutenticacao.EsquemaDeSessao, identidade);
    }

    public Task EncerrarAsync(CancellationToken cancelamento)
        => ContextoObrigatorio().SignOutAsync(ConfiguracaoDeAutenticacao.EsquemaDeSessao);

    public Task EncerrarTodasDoUsuarioAsync(Guid usuarioId, CancellationToken cancelamento)
        => _sessoes.RemoverDoUsuarioAsync(usuarioId, cancelamento);

    private HttpContext ContextoObrigatorio() => _acessor.HttpContext
        ?? throw new InvalidOperationException(
            "Sessão do painel manipulada fora de uma requisição HTTP; " +
            "esperado um HttpContext corrente para emitir ou apagar o cookie.");
}
