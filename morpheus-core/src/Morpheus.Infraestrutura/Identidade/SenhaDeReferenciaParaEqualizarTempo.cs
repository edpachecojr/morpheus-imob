using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Um hash de senha descartável, derivado de bytes aleatórios na primeira vez que
/// é pedido, contra o qual o login confere a senha quando <b>não existe conta</b>.
/// É o que faz "e-mail inexistente" custar o mesmo tempo que "senha errada".
/// <para>
/// O segredo nasce em memória a cada processo e nunca é gravado, logado ou
/// comparado com senha de usuário — ele não protege nada, só gasta o mesmo tempo.
/// </para>
/// </summary>
public sealed class SenhaDeReferenciaParaEqualizarTempo
{
    private readonly Lazy<string> _hash;

    public SenhaDeReferenciaParaEqualizarTempo(IPasswordHasher<UsuarioDaOrganizacao> hasher)
        => _hash = new Lazy<string>(() => hasher.HashPassword(
            new UsuarioDaOrganizacao(), Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))));

    public string Hash => _hash.Value;
}
