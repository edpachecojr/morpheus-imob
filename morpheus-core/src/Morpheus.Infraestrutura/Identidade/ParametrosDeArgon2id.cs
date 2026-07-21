namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Custo do Argon2id: memória, iterações e paralelismo. Fica num tipo próprio
/// porque é o que precisa subir com o tempo — e porque o hash gravado carrega os
/// parâmetros com que foi feito, permitindo reconhecer senha guardada com custo
/// antigo e reidratá-la no próximo login.
/// </summary>
public sealed record ParametrosDeArgon2id(int MemoriaEmKib, int Iteracoes, int Paralelismo)
{
    /// <summary>
    /// Perfil recomendado pelo OWASP para Argon2id em 2025: 19 MiB, 2 passagens,
    /// 1 thread. Trocar estes números não invalida senha nenhuma — muda só o custo
    /// das próximas, e as antigas são regravadas quando seus donos logam.
    /// </summary>
    public static readonly ParametrosDeArgon2id Atuais = new(MemoriaEmKib: 19456, Iteracoes: 2, Paralelismo: 1);

    /// <summary>Tamanho do sal, em bytes. 16 é o mínimo recomendado pela RFC 9106.</summary>
    public const int TamanhoDoSal = 16;

    /// <summary>Tamanho do resumo, em bytes.</summary>
    public const int TamanhoDoResumo = 32;
}
