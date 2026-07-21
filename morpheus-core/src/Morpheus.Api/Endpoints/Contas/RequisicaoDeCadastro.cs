using Morpheus.Aplicacao.Contas;

namespace Morpheus.Api.Endpoints.Contas;

/// <summary>
/// Corpo do formulário público de cadastro. Não tem — e nunca terá — campo de
/// organização, papel ou tenant: tudo isso vem do servidor, e aceitá-lo do cliente
/// seria vetor de escalada ([multi-tenancy.md](../../../../docs/fundacao/multi-tenancy.md), regra 4).
/// </summary>
/// <param name="PaginaPessoal">
/// Armadilha para robô. É um campo que o formulário mantém invisível ao humano,
/// então só um preenchedor automático de campos o preenche. Vindo com valor, a
/// requisição é descartada antes de tocar banco ou hash de senha — barato para
/// nós, indistinguível de sucesso para quem enviou.
/// </param>
public sealed record RequisicaoDeCadastro(
    string NomeCompleto,
    string Email,
    string Senha,
    string ConfirmacaoDeSenha,
    string? PaginaPessoal)
{
    public bool ArmadilhaPreenchida => !string.IsNullOrWhiteSpace(PaginaPessoal);

    public DadosDoCadastro ParaDados() =>
        new(NomeCompleto ?? string.Empty, Email ?? string.Empty, Senha ?? string.Empty, ConfirmacaoDeSenha ?? string.Empty);
}
