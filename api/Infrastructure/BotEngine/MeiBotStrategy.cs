using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.BotEngine;

public class MeiBotStrategy : IBotStrategy
{
    public BotType SupportedType => BotType.Mei;

    public Task<string> ProcessMessageAsync(User user, string message, CancellationToken ct = default)
    {
        var lower = message.ToLowerInvariant();

        var reply = lower switch
        {
            var m when m.Contains("das simples") || m.Contains("imposto") =>
                "💼 O DAS Simples Nacional vence todo dia 20 do mês! Emita sua guia no Portal PGMEI.",
            var m when m.Contains("nota") || m.Contains("nfe") || m.Contains("nfs") =>
                "📄 MEIs prestadores de serviço emitem NFS-e. Emitentes de produtos emitem NF-e. Precisa de ajuda com emissão?",
            var m when m.Contains("limite") || m.Contains("faturamento") =>
                "📊 O limite anual do MEI é R$ 81.000,00 (ou R$ 6.750/mês). Ultrapassar esse teto pode desenquadrar o MEI.",
            var m when m.Contains("funcionário") || m.Contains("empregado") =>
                "👷 O MEI pode contratar apenas 1 funcionário que receba salário mínimo ou o piso da categoria.",
            _ => "Olá MEI! 👋 Posso ajudar com:\n• Impostos e DAS\n• Emissão de Notas Fiscais\n• Limite de faturamento\n• Funcionário MEI\n\nDigite sua pergunta!"
        };

        return Task.FromResult(reply);
    }
}
