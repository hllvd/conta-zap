using Domain.Entities;

namespace Application.Interfaces;

public interface IGoogleSheetService
{
    Task<string> CreateSheetForUserAsync(User user, string templateId, CancellationToken ct = default);
    Task AppendTransactionAsync(string sheetId, string type, decimal amount, string category, string description, CancellationToken ct = default);
    Task<IList<IList<object>>> ReadSheetDataAsync(string sheetId, string range, CancellationToken ct = default);
}
