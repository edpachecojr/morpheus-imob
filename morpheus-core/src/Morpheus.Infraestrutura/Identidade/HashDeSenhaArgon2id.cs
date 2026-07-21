using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Guarda senha com Argon2id, substituindo o PBKDF2 padrão do Identity —
/// exigência de [autenticacao.md](../../../../docs/fundacao/autenticacao.md), que
/// recusa qualquer hash barato de calcular em GPU.
/// <para>
/// Substituir o <see cref="IPasswordHasher{TUser}"/> é o gancho oficial do
/// Identity para isso: o <c>UserManager</c> continua sendo quem chama, e nenhum
/// caso de uso fica sabendo qual algoritmo está por baixo.
/// </para>
/// </summary>
public sealed class HashDeSenhaArgon2id : IPasswordHasher<UsuarioDaOrganizacao>
{
    private readonly ParametrosDeArgon2id _parametros;

    public HashDeSenhaArgon2id() : this(ParametrosDeArgon2id.Atuais)
    {
    }

    public HashDeSenhaArgon2id(ParametrosDeArgon2id parametros) => _parametros = parametros;

    public string HashPassword(UsuarioDaOrganizacao usuario, string senha)
    {
        var sal = RandomNumberGenerator.GetBytes(ParametrosDeArgon2id.TamanhoDoSal);
        var resumo = Derivar(senha, sal, _parametros);
        return CodificacaoDeHashArgon2id.Codificar(_parametros, sal, resumo);
    }

    /// <summary>
    /// Confere a senha e sinaliza regravação quando o hash guardado usou custo
    /// diferente do atual — é assim que uma senha antiga se atualiza sozinha no
    /// primeiro login depois de subirmos os parâmetros.
    /// </summary>
    public PasswordVerificationResult VerifyHashedPassword(
        UsuarioDaOrganizacao usuario, string hashGuardado, string senhaFornecida)
    {
        if (!CodificacaoDeHashArgon2id.TentarDecodificar(hashGuardado, out var guardado))
            return PasswordVerificationResult.Failed;

        var calculado = Derivar(senhaFornecida, guardado.Sal, guardado.Parametros);
        if (!CryptographicOperations.FixedTimeEquals(calculado, guardado.Resumo))
            return PasswordVerificationResult.Failed;

        return guardado.Parametros == _parametros
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.SuccessRehashNeeded;
    }

    private static byte[] Derivar(string senha, byte[] sal, ParametrosDeArgon2id parametros)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(senha))
        {
            Salt = sal,
            MemorySize = parametros.MemoriaEmKib,
            Iterations = parametros.Iteracoes,
            DegreeOfParallelism = parametros.Paralelismo,
        };
        return argon.GetBytes(ParametrosDeArgon2id.TamanhoDoResumo);
    }
}
