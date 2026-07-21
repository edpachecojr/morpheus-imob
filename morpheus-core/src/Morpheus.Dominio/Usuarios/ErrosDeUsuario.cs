using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Catálogo dos erros de negócio do usuário do painel e da resolução de sua
/// identidade. Concentra os códigos que o tratamento de erro traduz em resposta.
/// </summary>
public static class ErrosDeUsuario
{
    public static readonly Erro NaoAutenticado =
        new("Usuario.NaoAutenticado",
            "Nenhum usuário autenticado no contexto: acesso a dados negado por padrão.");

    public static readonly Erro NomeCompletoObrigatorio =
        new("Usuario.NomeCompletoObrigatorio", "Nome completo não pode ser vazio.");

    public static readonly Erro IdentificadorObrigatorio =
        new("Usuario.IdentificadorObrigatorio", "UsuarioId não pode ser vazio.");

    public static Erro OrganizacaoNaoEncontrada(Guid usuarioId) =>
        new("Usuario.OrganizacaoNaoEncontrada", $"Nenhuma organização vinculada ao usuário {usuarioId}.");
}
