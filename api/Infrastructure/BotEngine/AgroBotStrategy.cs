using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.BotEngine;

public class AgroBotStrategy : IBotStrategy
{
    public BotType SupportedType => BotType.Agro;

    public Task<string> ProcessMessageAsync(User user, string message, CancellationToken ct = default)
    {
        var lower = message.ToLowerInvariant();

        var reply = lower switch
        {
            var m when m.Contains("clima") || m.Contains("chuva") || m.Contains("previsão") =>
                "🌤️ Para previsão do tempo agrícola, consulte o INMET: https://www.inmet.gov.br ou o app ClimaTempo Agro.",
            var m when m.Contains("soja") || m.Contains("milho") || m.Contains("preço") || m.Contains("cotação") =>
                "📈 Consulte cotações em tempo real na Agrolink: https://www.agrolink.com.br/cotacoes",
            var m when m.Contains("crédito") || m.Contains("financiamento") || m.Contains("pronaf") =>
                "🏦 O PRONAF oferece crédito a juros reduzidos para agricultores familiares. Procure um banco credenciado com sua DAP/CAF.",
            var m when m.Contains("plantio") || m.Contains("época") || m.Contains("safra") =>
                "🌱 A época de plantio varia por cultura e região. Consulte o Zoneamento Agrícola do MAPA: https://www.mapa.gov.br/zarc",
            _ => "🌾 Olá! Sou o AgroBot. Posso ajudar com:\n• Previsão do tempo\n• Cotações (soja, milho...)\n• Crédito rural e PRONAF\n• Épocas de plantio\n\nDigite sua pergunta!"
        };

        return Task.FromResult(reply);
    }
}
