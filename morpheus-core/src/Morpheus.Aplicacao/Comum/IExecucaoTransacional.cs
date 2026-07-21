using Morpheus.Dominio.Resultados;

namespace Morpheus.Aplicacao.Comum;

/// <summary>
/// Executa um bloco de escritas como uma transação só: ou todas as gravações
/// entram, ou nenhuma entra. Interface fina de propriedade nossa sobre o
/// mecanismo transacional do provedor de dados — o caso de uso declara o limite
/// da atomicidade sem conhecer EF Core, Npgsql ou nível de isolamento.
/// <para>
/// Existe porque o cadastro cria organização e usuário em gravações separadas
/// (o store do Identity grava por conta própria) e um cadastro pela metade — org
/// sem dono ou dono sem org — é pior do que cadastro nenhum (E1-F1-H2).
/// </para>
/// </summary>
public interface IExecucaoTransacional
{
    /// <summary>
    /// Roda a operação numa transação. Confirma quando ela devolve
    /// <see cref="Resultado.Sucesso"/>; desfaz quando devolve falha ou lança — o
    /// <see cref="Resultado"/> de falha é desfecho esperado e mesmo assim não
    /// pode deixar gravação pela metade. Exemplo:
    /// <c>await _transacao.ExecutarAsync(ct => FundarOrganizacaoComDonoAsync(dados, ct), cancelamento)</c>.
    /// </summary>
    Task<Resultado> ExecutarAsync(
        Func<CancellationToken, Task<Resultado>> operacao, CancellationToken cancelamento);
}
