using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Catálogo dos erros de autenticação. Deliberadamente pobre em detalhe: senha
/// errada, e-mail inexistente e conta bloqueada devolvem o <b>mesmo</b> erro, para
/// que a resposta não diga a um atacante quais e-mails existem
/// ([autenticacao.md](../../../../docs/fundacao/autenticacao.md)). O detalhe do
/// motivo real vive no log, nunca na resposta.
/// </summary>
public static class ErrosDeAutenticacao
{
    /// <summary>A única recusa de login que o cliente vê, seja qual for o motivo real.</summary>
    public static readonly Erro CredenciaisInvalidas =
        new("Autenticacao.CredenciaisInvalidas", "E-mail ou senha inválidos.");

    // Os três motivos reais de recusa. São de uso interno — servem ao log, que
    // precisa distinguir ataque de esquecimento — e nunca entram na resposta.
    public static readonly Erro ContaInexistente =
        new("Autenticacao.ContaInexistente", "Nenhuma conta com o e-mail informado.");

    public static readonly Erro SenhaIncorreta =
        new("Autenticacao.SenhaIncorreta", "Senha não confere com a da conta.");

    public static readonly Erro ContaBloqueada =
        new("Autenticacao.ContaBloqueada", "Conta bloqueada por tentativas de acesso malsucedidas.");

    public static readonly Erro TokenDeRecuperacaoInvalido =
        new("Autenticacao.TokenDeRecuperacaoInvalido",
            "Link de redefinição inválido ou expirado; solicite um novo.");

    public static readonly Erro TokenDeConfirmacaoInvalido =
        new("Autenticacao.TokenDeConfirmacaoInvalido",
            "Link de confirmação inválido ou expirado; solicite um novo.");
}
