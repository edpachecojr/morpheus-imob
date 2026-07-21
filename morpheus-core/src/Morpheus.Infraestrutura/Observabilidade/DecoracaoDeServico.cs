using Microsoft.Extensions.DependencyInjection;

namespace Morpheus.Infraestrutura.Observabilidade;

/// <summary>
/// Substitui o registro de um serviço por um decorador que o envolve, preservando
/// o tempo de vida original. Permite adicionar comportamento transversal (log,
/// métrica) sem alterar quem consome o serviço nem quem o implementa (OCP), e sem
/// depender de mediator ou de biblioteca externa de interceptação.
///
/// Uso:
/// <c>servicos.Decorar&lt;IConsultaDeImoveisResumidos, ConsultaDeImoveisComRegistroDeLog&gt;();</c>
/// </summary>
public static class DecoracaoDeServico
{
    public static IServiceCollection Decorar<TServico, TDecorador>(this IServiceCollection servicos)
        where TServico : class
        where TDecorador : class, TServico
    {
        var original = EncontrarRegistro<TServico>(servicos);
        servicos.Remove(original);
        servicos.Add(new ServiceDescriptor(
            typeof(TServico),
            provedor => CriarDecorador<TServico, TDecorador>(provedor, original),
            original.Lifetime));
        return servicos;
    }

    private static ServiceDescriptor EncontrarRegistro<TServico>(IServiceCollection servicos) =>
        servicos.LastOrDefault(descritor => descritor.ServiceType == typeof(TServico))
            ?? throw new InvalidOperationException(
                $"Não há registro de {typeof(TServico).Name} para decorar. " +
                "Registre o serviço antes de chamar Decorar.");

    private static TDecorador CriarDecorador<TServico, TDecorador>(
        IServiceProvider provedor, ServiceDescriptor original)
        where TServico : class
        where TDecorador : class, TServico
    {
        var interno = (TServico)InstanciarOriginal(provedor, original);
        return ActivatorUtilities.CreateInstance<TDecorador>(provedor, interno);
    }

    private static object InstanciarOriginal(IServiceProvider provedor, ServiceDescriptor original)
    {
        if (original.ImplementationInstance is not null)
            return original.ImplementationInstance;
        if (original.ImplementationFactory is not null)
            return original.ImplementationFactory(provedor);
        return ActivatorUtilities.CreateInstance(provedor, original.ImplementationType!);
    }
}
