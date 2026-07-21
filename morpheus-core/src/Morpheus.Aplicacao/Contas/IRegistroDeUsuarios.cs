using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Contas;

/// <summary>
/// Criação de usuários do painel com senha e papel. Interface fina sobre o store
/// do Identity: o caso de uso não conhece <c>UserManager</c>, <c>IdentityResult</c>
/// nem código de erro de SDK — recebe o vocabulário de falha do projeto.
/// </summary>
public interface IRegistroDeUsuarios
{
    /// <summary>
    /// Aplica as regras de força de senha <b>sem</b> criar usuário. Roda antes da
    /// checagem de e-mail existente de propósito: se a senha fraca só fosse
    /// recusada no caminho de e-mail novo, a diferença de resposta viraria oráculo
    /// de enumeração de contas.
    /// </summary>
    Task<Resultado> ValidarSenhaAsync(string senha, CancellationToken cancelamento);

    /// <summary>Se já existe usuário com o e-mail. O resultado nunca vai para a resposta HTTP.</summary>
    Task<bool> ExisteComEmailAsync(string email, CancellationToken cancelamento);

    /// <summary>
    /// Cria o usuário, define a senha e o vincula ao papel. Falha com
    /// <see cref="Morpheus.Dominio.Usuarios.ErrosDeCadastro.EmailJaCadastrado"/>
    /// quando outra requisição ganhou a corrida pelo mesmo e-mail.
    /// </summary>
    Task<Resultado<Guid>> CriarAsync(NovoUsuarioDoPainel novo, CancellationToken cancelamento);
}
