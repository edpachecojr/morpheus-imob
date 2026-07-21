using System.Globalization;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Traduz entre o hash em memória e a string PHC gravada no banco:
/// <c>$argon2id$v=19$m=19456,t=2,p=1$&lt;sal&gt;$&lt;resumo&gt;</c>. Formato padrão de fato,
/// e não um layout nosso, para que a senha continue verificável por qualquer
/// ferramenta se um dia trocarmos de biblioteca.
/// <para>
/// Lógica pura, sem criptografia: é o que torna o formato testável sem pagar o
/// custo de derivar uma chave por caso de teste.
/// </para>
/// </summary>
public static class CodificacaoDeHashArgon2id
{
    private const string Identificador = "argon2id";
    private const int VersaoDoAlgoritmo = 19;

    public static string Codificar(ParametrosDeArgon2id parametros, byte[] sal, byte[] resumo) =>
        string.Create(CultureInfo.InvariantCulture,
            $"${Identificador}$v={VersaoDoAlgoritmo}$m={parametros.MemoriaEmKib},t={parametros.Iteracoes}," +
            $"p={parametros.Paralelismo}${Convert.ToBase64String(sal)}${Convert.ToBase64String(resumo)}");

    /// <summary>
    /// Decodifica a string gravada. Devolve <c>false</c> — em vez de lançar — para
    /// qualquer coisa fora do formato: hash corrompido no banco é caso de recusar o
    /// login, não de derrubar a requisição com exceção.
    /// </summary>
    public static bool TentarDecodificar(string codificado, out HashArgon2idDecodificado decodificado)
    {
        decodificado = default!;
        var partes = codificado.Split('$');
        if (partes.Length != 6 || partes[1] != Identificador)
            return false;

        if (!TentarLerCustos(partes[3], out var parametros))
            return false;

        try
        {
            decodificado = new HashArgon2idDecodificado(
                parametros, Convert.FromBase64String(partes[4]), Convert.FromBase64String(partes[5]));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TentarLerCustos(string custos, out ParametrosDeArgon2id parametros)
    {
        parametros = default!;
        var campos = custos.Split(',');
        if (campos.Length != 3)
            return false;

        if (!TentarLerNumero(campos[0], "m=", out var memoria) ||
            !TentarLerNumero(campos[1], "t=", out var iteracoes) ||
            !TentarLerNumero(campos[2], "p=", out var paralelismo))
            return false;

        parametros = new ParametrosDeArgon2id(memoria, iteracoes, paralelismo);
        return true;
    }

    private static bool TentarLerNumero(string campo, string prefixo, out int valor)
    {
        valor = 0;
        return campo.StartsWith(prefixo, StringComparison.Ordinal)
            && int.TryParse(campo[prefixo.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out valor);
    }
}

/// <summary>Partes de um hash Argon2id lido do banco.</summary>
public sealed record HashArgon2idDecodificado(ParametrosDeArgon2id Parametros, byte[] Sal, byte[] Resumo);
