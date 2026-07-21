using Microsoft.AspNetCore.Identity;
using Morpheus.Aplicacao.Senhas;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Emite e consome o token de redefinição pelo provedor de tokens do Identity.
/// O token é assinado e protegido pelo Data Protection, com validade configurada
/// em <c>ConfiguracaoDeInfraestrutura</c> — nada é guardado no banco, então não
/// existe tabela de token para vazar.
/// </summary>
public sealed class TokensDeRecuperacaoComIdentity : ITokensDeRecuperacaoDeSenha
{
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;

    public TokensDeRecuperacaoComIdentity(UserManager<UsuarioDaOrganizacao> usuarios) => _usuarios = usuarios;

    public async Task<string> GerarAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        // Chegar aqui sem usuário só acontece se ele sumir entre a busca por e-mail
        // e esta chamada — estado impossível no fluxo normal, então falha alto.
        var usuario = await BuscarAsync(usuarioId)
            ?? throw new InvalidOperationException(
                $"Token de recuperação pedido para o usuário {usuarioId}, que não existe mais no registro.");
        return await _usuarios.GeneratePasswordResetTokenAsync(usuario);
    }

    public async Task<Resultado> RedefinirAsync(
        Guid usuarioId, string token, string novaSenha, CancellationToken cancelamento)
    {
        var usuario = await BuscarAsync(usuarioId);
        if (usuario is null)
            return Resultado.DeFalha(ErrosDeAutenticacao.TokenDeRecuperacaoInvalido);

        var redefinicao = await _usuarios.ResetPasswordAsync(usuario, token, novaSenha);
        if (redefinicao.Succeeded)
            return Resultado.DeSucesso();

        return Resultado.DeFalha(EhSenhaFraca(redefinicao)
            ? ErrosDeCadastro.SenhaRecusada(Descrever(redefinicao))
            : ErrosDeAutenticacao.TokenDeRecuperacaoInvalido);
    }

    private Task<UsuarioDaOrganizacao?> BuscarAsync(Guid usuarioId)
        => _usuarios.FindByIdAsync(usuarioId.ToString());

    // Senha fraca merece explicação; qualquer outra recusa vira "token inválido",
    // que é a mensagem genérica que não distingue token expirado de token forjado.
    private static bool EhSenhaFraca(IdentityResult resultado)
        => resultado.Errors.Any(erro => erro.Code.StartsWith("Password", StringComparison.Ordinal));

    private static string Descrever(IdentityResult resultado)
        => string.Join(" ", resultado.Errors.Select(erro => erro.Description));
}
