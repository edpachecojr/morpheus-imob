namespace Morpheus.Api.Seguranca;

/// <summary>
/// Quantas requisições de autenticação uma mesma origem pode fazer por minuto.
/// Configurável porque o número certo depende de estar atrás de proxy, de CDN ou
/// direto na internet — e porque um teste precisa poder apertá-lo para provar que
/// o limite existe.
/// </summary>
public sealed class OpcoesDeLimiteDeAutenticacao
{
    public const string Secao = "LimiteDeAutenticacao";

    /// <summary>
    /// Teto por origem, por minuto. O padrão é folgado de propósito: a defesa
    /// principal contra força bruta é o bloqueio por conta (5 erros), e um teto
    /// baixo demais por IP derruba escritório inteiro atrás de um NAT só.
    /// </summary>
    public int RequisicoesPorMinuto { get; set; } = 20;
}
