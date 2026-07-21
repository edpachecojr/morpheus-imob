using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Catálogo dos erros do cadastro de conta (E1-F1-H2). Quase todos descrevem
/// entrada que o próprio solicitante digitou e pode corrigir — por isso dizem o
/// formato esperado. A exceção é <see cref="EmailJaCadastrado"/>, que é de uso
/// exclusivamente interno: o cadastro o converte em resposta de sucesso, porque
/// revelar que a conta existe é enumeração.
/// </summary>
public static class ErrosDeCadastro
{
    /// <summary>
    /// Uso interno: nunca chega ao cliente. O caso de uso o traduz na mesma
    /// resposta do cadastro bem-sucedido.
    /// </summary>
    public static readonly Erro EmailJaCadastrado =
        new("Cadastro.EmailJaCadastrado", "Já existe conta com este e-mail.");

    public static readonly Erro SenhasNaoConferem =
        new("Cadastro.SenhasNaoConferem",
            "A confirmação de senha não confere com a senha informada.");

    public static Erro EmailInvalido(string email) =>
        new("Cadastro.EmailInvalido",
            $"E-mail '{email}' é inválido; esperado o formato 'nome@dominio.com'.");

    public static Erro SenhaRecusada(string motivos) =>
        new("Cadastro.SenhaRecusada", $"Senha recusada: {motivos}");

    public static Erro CadastroRecusado(string motivos) =>
        new("Cadastro.Recusado", $"Cadastro recusado: {motivos}");
}
