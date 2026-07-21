using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Catálogo dos erros de negócio da organização (o tenant). Separado dos erros
/// de isolamento: aqui moram as regras de criação; lá, as barreiras entre tenants.
/// </summary>
public static class ErrosDeOrganizacao
{
    public static readonly Erro NomeObrigatorio =
        new("Organizacao.NomeObrigatorio", "Nome da organização não pode ser vazio.");

    public static readonly Erro FusoHorarioObrigatorio =
        new("Organizacao.FusoHorarioObrigatorio",
            "Fuso horário não pode ser vazio; esperado um id IANA como 'America/Sao_Paulo'.");

    public static Erro JanelaDeAtendimentoInvertida(TimeOnly inicio, TimeOnly fim) =>
        new("Organizacao.JanelaDeAtendimentoInvertida",
            $"Janela de atendimento inválida: fim {fim:HH\\:mm} não é posterior ao início {inicio:HH\\:mm}.");
}
