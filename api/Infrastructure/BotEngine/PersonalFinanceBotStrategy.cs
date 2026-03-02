using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.BotEngine;

public class PersonalFinanceBotStrategy(IGoogleSheetService sheetService) : IBotStrategy
{
    public BotType SupportedType => BotType.PersonalFinance;

    public async Task<string> ProcessMessageAsync(User user, string message, CancellationToken ct = default)
    {
        if (user.GoogleSheetId is null)
            return "⚠️ Sua planilha ainda está sendo configurada. Tente novamente em instantes.";

        var intent = ParseIntent(message);

        return intent switch
        {
            Intent.RegisterExpense  => await HandleRegisterExpenseAsync(user, message, ct),
            Intent.RegisterIncome   => await HandleRegisterIncomeAsync(user, message, ct),
            Intent.QuerySpending    => await HandleQuerySpendingAsync(user, message, ct),
            Intent.QueryIncome      => await HandleQueryIncomeAsync(user, message, ct),
            Intent.MonthlySummary   => await HandleMonthlySummaryAsync(user, message, ct),
            Intent.Help             => BuildHelpMessage(),
            _                       => "Não entendi 🤔 Tente: 'gastei R$50 em alimentação' ou 'quanto gastei esse mês?'\n\nDigite *ajuda* para ver todos os comandos."
        };
    }

    private static Intent ParseIntent(string message)
    {
        var lower = message.ToLowerInvariant();
        if (lower.Contains("gastei") || lower.Contains("gasto") || lower.Contains("paguei"))
            return Intent.RegisterExpense;
        if (lower.Contains("recebi") || lower.Contains("receita") || lower.Contains("salário") || lower.Contains("salario"))
            return Intent.RegisterIncome;
        if (lower.Contains("quanto gastei") || lower.Contains("total gasto") || lower.Contains("gastos"))
            return Intent.QuerySpending;
        if (lower.Contains("quanto recebi") || lower.Contains("total receita"))
            return Intent.QueryIncome;
        if (lower.Contains("resumo") || lower.Contains("extrato") || lower.Contains("relatório") || lower.Contains("relatorio"))
            return Intent.MonthlySummary;
        if (lower.Contains("ajuda") || lower.Contains("help") || lower.Contains("comandos"))
            return Intent.Help;
        return Intent.Unknown;
    }

    private async Task<string> HandleRegisterExpenseAsync(User user, string message, CancellationToken ct)
    {
        var (amount, category) = ExtractAmountAndCategory(message);
        if (amount <= 0) return "Não consegui identificar o valor 😕 Tente: 'gastei R$50 em alimentação'";

        await sheetService.AppendTransactionAsync(
            user.GoogleSheetId!, "despesa", amount, category, message, ct);

        return $"✅ Despesa registrada!\n💸 R$ {amount:F2} em *{category}*";
    }

    private async Task<string> HandleRegisterIncomeAsync(User user, string message, CancellationToken ct)
    {
        var (amount, category) = ExtractAmountAndCategory(message, defaultCategory: "Receita");
        if (amount <= 0) return "Não consegui identificar o valor 😕 Tente: 'recebi R$5000 de salário'";

        await sheetService.AppendTransactionAsync(
            user.GoogleSheetId!, "receita", amount, category, message, ct);

        return $"✅ Receita registrada!\n💰 R$ {amount:F2} em *{category}*";
    }

    private async Task<string> HandleQuerySpendingAsync(User user, string message, CancellationToken ct)
    {
        var month = ExtractMonth(message) ?? DateTimeOffset.UtcNow.Month;
        var data = await sheetService.ReadSheetDataAsync(user.GoogleSheetId!, "Transações!A:C", ct);

        var total = data.Skip(1)
            .Where(row => row.Count >= 3
                && row[1]?.ToString() == "despesa"
                && DateTimeOffset.TryParse(row[0]?.ToString(), out var d)
                && d.Month == month)
            .Sum(row => decimal.TryParse(row[2]?.ToString(), out var v) ? v : 0);

        return $"📊 Total de despesas: R$ {total:F2} (mês {month})";
    }

    private async Task<string> HandleQueryIncomeAsync(User user, string message, CancellationToken ct)
    {
        var month = ExtractMonth(message) ?? DateTimeOffset.UtcNow.Month;
        var data = await sheetService.ReadSheetDataAsync(user.GoogleSheetId!, "Transações!A:C", ct);

        var total = data.Skip(1)
            .Where(row => row.Count >= 3
                && row[1]?.ToString() == "receita"
                && DateTimeOffset.TryParse(row[0]?.ToString(), out var d)
                && d.Month == month)
            .Sum(row => decimal.TryParse(row[2]?.ToString(), out var v) ? v : 0);

        return $"📊 Total de receitas: R$ {total:F2} (mês {month})";
    }

    private async Task<string> HandleMonthlySummaryAsync(User user, string message, CancellationToken ct)
    {
        var month = ExtractMonth(message) ?? DateTimeOffset.UtcNow.Month;
        var data = await sheetService.ReadSheetDataAsync(user.GoogleSheetId!, "Transações!A:C", ct);

        var rows = data.Skip(1)
            .Where(row => row.Count >= 3
                && DateTimeOffset.TryParse(row[0]?.ToString(), out var d)
                && d.Month == month)
            .ToList();

        var totalExpenses = rows
            .Where(r => r[1]?.ToString() == "despesa")
            .Sum(r => decimal.TryParse(r[2]?.ToString(), out var v) ? v : 0);
        var totalIncome = rows
            .Where(r => r[1]?.ToString() == "receita")
            .Sum(r => decimal.TryParse(r[2]?.ToString(), out var v) ? v : 0);
        var balance = totalIncome - totalExpenses;

        return $"""
            📋 *Resumo do mês {month}*
            ──────────────────
            💰 Receitas:  R$ {totalIncome:F2}
            💸 Despesas:  R$ {totalExpenses:F2}
            💳 Saldo:     R$ {balance:F2} {(balance >= 0 ? "✅" : "⚠️")}
            ──────────────────
            🔗 Sua planilha completa está disponível no Google Sheets!
            """;
    }

    private static string BuildHelpMessage() =>
        """
        🤖 *Comandos disponíveis:*
        ──────────────────
        💸 Registrar gasto:
           "gastei R$50 em alimentação"
        💰 Registrar receita:
           "recebi R$5000 de salário"
        📊 Ver gastos do mês:
           "quanto gastei em março?"
        📈 Ver receitas do mês:
           "quanto recebi esse mês?"
        📋 Resumo mensal:
           "me dá um resumo do mês"
        ──────────────────
        """;

    private static (decimal Amount, string Category) ExtractAmountAndCategory(
        string message, string defaultCategory = "Outros")
    {
        // Extract R$ value
        var amountMatch = System.Text.RegularExpressions.Regex.Match(
            message, @"R?\$?\s*(\d+(?:[.,]\d{1,2})?)");
        decimal amount = 0;
        if (amountMatch.Success)
            decimal.TryParse(amountMatch.Groups[1].Value.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out amount);

        // Extract category keyword
        var categoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "alimentação", "Alimentação" }, { "comida", "Alimentação" }, { "restaurante", "Alimentação" },
            { "transporte", "Transporte" }, { "uber", "Transporte" }, { "gasolina", "Transporte" },
            { "saúde", "Saúde" }, { "remédio", "Saúde" }, { "médico", "Saúde" },
            { "lazer", "Lazer" }, { "cinema", "Lazer" }, { "viagem", "Lazer" },
            { "moradia", "Moradia" }, { "aluguel", "Moradia" }, { "condomínio", "Moradia" },
            { "salário", "Salário" }, { "freelance", "Freelance" }, { "receita", "Receita" }
        };

        var category = defaultCategory;
        var lower = message.ToLowerInvariant();
        foreach (var (keyword, cat) in categoryMap)
        {
            if (lower.Contains(keyword.ToLowerInvariant()))
            {
                category = cat;
                break;
            }
        }

        return (amount, category);
    }

    private static int? ExtractMonth(string message)
    {
        var months = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "janeiro", 1 }, { "fevereiro", 2 }, { "março", 3 }, { "abril", 4 },
            { "maio", 5 }, { "junho", 6 }, { "julho", 7 }, { "agosto", 8 },
            { "setembro", 9 }, { "outubro", 10 }, { "novembro", 11 }, { "dezembro", 12 }
        };
        foreach (var (name, num) in months)
            if (message.Contains(name, StringComparison.OrdinalIgnoreCase))
                return num;
        return null;
    }

    private enum Intent
    {
        RegisterExpense, RegisterIncome, QuerySpending, QueryIncome, MonthlySummary, Help, Unknown
    }
}
