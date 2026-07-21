using Microsoft.AspNetCore.Identity;
using Morpheus.Aplicacao.Contas;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Cria usuários do painel pelo <c>UserManager</c>, traduzindo
/// <see cref="IdentityResult"/> para o vocabulário de falha do projeto. É a única
/// classe que conhece os códigos de erro do SDK.
/// </summary>
public sealed class RegistroDeUsuariosComIdentity : IRegistroDeUsuarios
{
    private readonly UserManager<UsuarioDaOrganizacao> _usuarios;
    private readonly TimeProvider _relogio;

    public RegistroDeUsuariosComIdentity(UserManager<UsuarioDaOrganizacao> usuarios, TimeProvider relogio)
    {
        _usuarios = usuarios;
        _relogio = relogio;
    }

    public async Task<Resultado> ValidarSenhaAsync(string senha, CancellationToken cancelamento)
    {
        var candidato = new UsuarioDaOrganizacao();
        foreach (var validador in _usuarios.PasswordValidators)
        {
            var resultado = await validador.ValidateAsync(_usuarios, candidato, senha);
            if (!resultado.Succeeded)
                return Resultado.DeFalha(ErrosDeCadastro.SenhaRecusada(Descrever(resultado)));
        }

        return Resultado.DeSucesso();
    }

    public async Task<bool> ExisteComEmailAsync(string email, CancellationToken cancelamento)
        => await _usuarios.FindByEmailAsync(email) is not null;

    public async Task<Resultado<Guid>> CriarAsync(NovoUsuarioDoPainel novo, CancellationToken cancelamento)
    {
        var usuario = UsuarioDaOrganizacao.Cadastrar(
            new OrganizacaoDona(novo.OrganizacaoId), novo.NomeCompleto, novo.Email, novo.Papel, _relogio);

        var criacao = await _usuarios.CreateAsync(usuario, novo.Senha);
        if (!criacao.Succeeded)
            return Traduzir(criacao);

        var vinculo = await _usuarios.AddToRoleAsync(usuario, novo.Papel);
        return vinculo.Succeeded ? usuario.Id : Traduzir(vinculo);
    }

    // Duplicidade de e-mail nunca é falha visível: o cadastro a converte em
    // sucesso para não confirmar a existência da conta.
    private static Resultado<Guid> Traduzir(IdentityResult resultado)
        => resultado.Errors.Any(EhDuplicidadeDeIdentificador)
            ? Resultado.DeFalha<Guid>(ErrosDeCadastro.EmailJaCadastrado)
            : Resultado.DeFalha<Guid>(ErrosDeCadastro.CadastroRecusado(Descrever(resultado)));

    private static bool EhDuplicidadeDeIdentificador(IdentityError erro)
        => erro.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
            or nameof(IdentityErrorDescriber.DuplicateUserName);

    private static string Descrever(IdentityResult resultado)
        => string.Join(" ", resultado.Errors.Select(erro => erro.Description));
}
