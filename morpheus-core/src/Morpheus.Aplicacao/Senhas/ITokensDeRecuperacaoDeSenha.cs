using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Senhas;

/// <summary>
/// Emissão e consumo do token de redefinição de senha. Interface fina sobre o
/// provedor de tokens do Identity: o caso de uso não sabe como o token é
/// construído, só que ele é de uso único e expira.
/// </summary>
public interface ITokensDeRecuperacaoDeSenha
{
    /// <summary>Emite um token de redefinição para o usuário. O token nunca é logado nem persistido.</summary>
    Task<string> GerarAsync(Guid usuarioId, CancellationToken cancelamento);

    /// <summary>
    /// Troca a senha consumindo o token. O token vale uma vez: redefinir a senha
    /// gira o carimbo de segurança da conta, o que invalida qualquer token ainda
    /// não usado, inclusive este.
    /// </summary>
    Task<Resultado> RedefinirAsync(
        Guid usuarioId, string token, string novaSenha, CancellationToken cancelamento);
}
