using Microsoft.AspNetCore.Identity;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Emite e consome o token de confirmação de e-mail pelo provedor de tokens do
/// Identity — o mesmo mecanismo assinado e protegido por Data Protection que a
/// recuperação de senha usa, sem tabela própria de token para vazar.
/// </summary>
public sealed class TokensDeConfirmacaoDeEmailComIdentity : ITokensDeConfirmacaoDeEmail
{
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;

    public TokensDeConfirmacaoDeEmailComIdentity(UserManager<UsuarioDaOrganizacao> usuarios) => _usuarios = usuarios;

    public async Task<string> GerarAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var usuario = await BuscarAsync(usuarioId);
        return await _usuarios.GenerateEmailConfirmationTokenAsync(usuario);
    }

    public async Task<string> RenovarAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        var usuario = await BuscarAsync(usuarioId);

        // O token do Identity é válido enquanto o carimbo de segurança do usuário
        // não mudar; girá-lo antes de emitir o novo invalida qualquer token
        // pendente — mesmo mecanismo que a redefinição de senha já usa (E1-F2-H3).
        await _usuarios.UpdateSecurityStampAsync(usuario);
        return await _usuarios.GenerateEmailConfirmationTokenAsync(usuario);
    }

    public async Task<Resultado> ConfirmarAsync(Guid usuarioId, string token, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.FindByIdAsync(usuarioId.ToString());
        if (usuario is null)
            return Resultado.DeFalha(ErrosDeAutenticacao.TokenDeConfirmacaoInvalido);

        var confirmacao = await _usuarios.ConfirmEmailAsync(usuario, token);
        return confirmacao.Succeeded
            ? Resultado.DeSucesso()
            : Resultado.DeFalha(ErrosDeAutenticacao.TokenDeConfirmacaoInvalido);
    }

    // Chegar aqui sem usuário só acontece se ele sumir entre a resolução da
    // sessão e esta chamada — estado impossível no fluxo normal, então falha alto.
    private async Task<UsuarioDaOrganizacao> BuscarAsync(Guid usuarioId)
        => await _usuarios.FindByIdAsync(usuarioId.ToString())
            ?? throw new InvalidOperationException(
                $"Token de confirmação pedido para o usuário {usuarioId}, que não existe mais no registro.");
}
