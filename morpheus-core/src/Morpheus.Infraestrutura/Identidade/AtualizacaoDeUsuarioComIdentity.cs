using Microsoft.AspNetCore.Identity;
using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Atualiza os próprios dados do usuário pelo <c>UserManager</c>. Reaproveita
/// <see cref="UsuarioDaOrganizacao.DefinirNomeCompleto"/> — a mesma regra do
/// cadastro vale para a edição, sem reescrevê-la aqui.
/// </summary>
public sealed class AtualizacaoDeUsuarioComIdentity : IAtualizacaoDeUsuario
{
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;

    public AtualizacaoDeUsuarioComIdentity(UserManager<UsuarioDaOrganizacao> usuarios) => _usuarios = usuarios;

    public async Task<Resultado> AtualizarNomeCompletoAsync(
        Guid usuarioId, string nomeCompleto, CancellationToken cancelamento)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            return Resultado.DeFalha(ErrosDeUsuario.NomeCompletoObrigatorio);

        // Chegar aqui sem usuário só acontece se a sessão sobreviver à exclusão da
        // conta — estado impossível no fluxo normal, então falha alto.
        var usuario = await _usuarios.FindByIdAsync(usuarioId.ToString())
            ?? throw new InvalidOperationException(
                $"Atualização de nome pedida para o usuário {usuarioId}, que não existe mais no registro.");

        usuario.DefinirNomeCompleto(nomeCompleto);
        await _usuarios.UpdateAsync(usuario);
        return Resultado.DeSucesso();
    }
}
