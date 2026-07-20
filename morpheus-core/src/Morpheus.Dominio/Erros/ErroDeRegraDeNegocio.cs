namespace Morpheus.Dominio.Erros;

/// <summary>
/// Falha de regra de negócio do domínio. A mensagem carrega o contexto
/// suficiente para o chamador entender o valor que causou a falha e o esperado.
/// Serve de raiz comum para o tratamento de erro traduzir domínio em resposta.
/// </summary>
public abstract class ErroDeRegraDeNegocio : Exception
{
    protected ErroDeRegraDeNegocio(string mensagem) : base(mensagem)
    {
    }
}
