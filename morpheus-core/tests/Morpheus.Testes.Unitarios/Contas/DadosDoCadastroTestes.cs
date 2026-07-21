using Morpheus.Aplicacao.Contas;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Testes.Unitarios.Contas;

public sealed class DadosDoCadastroTestes
{
    private const string SenhaValida = "uma-senha-bem-longa";

    [Fact]
    public void Cadastro_completo_e_valido()
        => Assert.True(Cadastro("Ana Souza", "ana@exemplo.com", SenhaValida, SenhaValida).Validar().Sucesso);

    [Fact]
    public void Nome_vazio_e_recusado()
    {
        var resultado = Cadastro("   ", "ana@exemplo.com", SenhaValida, SenhaValida).Validar();

        Assert.Equal(ErrosDeUsuario.NomeCompletoObrigatorio, resultado.Erro);
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("")]
    [InlineData("ana@")]
    public void Email_fora_do_formato_e_recusado_citando_o_valor(string email)
    {
        var resultado = Cadastro("Ana Souza", email, SenhaValida, SenhaValida).Validar();

        Assert.Equal("Cadastro.EmailInvalido", resultado.Erro.Codigo);
        Assert.Contains(email, resultado.Erro.Descricao, StringComparison.Ordinal);
    }

    [Fact]
    public void Confirmacao_diferente_e_recusada()
    {
        var resultado = Cadastro("Ana Souza", "ana@exemplo.com", SenhaValida, "outra-senha-longa").Validar();

        Assert.Equal(ErrosDeCadastro.SenhasNaoConferem, resultado.Erro);
    }

    private static DadosDoCadastro Cadastro(string nome, string email, string senha, string confirmacao)
        => new(nome, email, senha, confirmacao);
}
