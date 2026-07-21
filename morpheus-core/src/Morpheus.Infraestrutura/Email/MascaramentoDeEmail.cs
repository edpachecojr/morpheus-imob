namespace Morpheus.Infraestrutura.Email;

/// <summary>
/// Mascara o e-mail para log — "ana.souza@exemplo.com" vira "a***@exemplo.com":
/// o suficiente para casar com um chamado de suporte, insuficiente para
/// reconstruir a lista de clientes (CLAUDE.md §Logging).
/// </summary>
internal static class MascaramentoDeEmail
{
    public static string Mascarar(string email)
    {
        var arroba = email.IndexOf('@', StringComparison.Ordinal);
        return arroba <= 0 ? "***" : $"{email[0]}***{email[arroba..]}";
    }
}
