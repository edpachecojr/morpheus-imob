using System.Net.Mail;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Contas;

/// <summary>
/// Entrada do cadastro de conta, como o solicitante a digitou. Não tem campo de
/// organização nem de papel: quem se cadastra vira dono da própria organização e
/// nada disso vem do cliente (regra 4 de [multi-tenancy.md](../../../../docs/fundacao/multi-tenancy.md)).
/// </summary>
public sealed record DadosDoCadastro(
    string NomeCompleto,
    string Email,
    string Senha,
    string ConfirmacaoDeSenha)
{
    /// <summary>
    /// Valida o que dá para validar sem tocar em infraestrutura: nome presente,
    /// e-mail no formato e confirmação conferindo. A força da senha fica com o
    /// registro de usuários, que é dono dessa política.
    /// Exemplo: <c>dados.Validar()</c>.
    /// </summary>
    public Resultado Validar()
    {
        if (string.IsNullOrWhiteSpace(NomeCompleto))
            return Resultado.DeFalha(ErrosDeUsuario.NomeCompletoObrigatorio);
        if (!MailAddress.TryCreate(Email, out _))
            return Resultado.DeFalha(ErrosDeCadastro.EmailInvalido(Email));
        if (!string.Equals(Senha, ConfirmacaoDeSenha, StringComparison.Ordinal))
            return Resultado.DeFalha(ErrosDeCadastro.SenhasNaoConferem);
        return Resultado.DeSucesso();
    }
}
