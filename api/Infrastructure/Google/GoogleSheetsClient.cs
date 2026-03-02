using Application.Interfaces;
using DomainUser = Domain.Entities.User;
using DriveData = Google.Apis.Drive.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Google;

public class GoogleSheetsClient : IGoogleSheetService
{
    private readonly string? _keyPath;
    private readonly string? _folderId;

    // Lazily initialized — so a missing/invalid service-account.json
    // only fails the operations that actually need Google, not all endpoints.
    private SheetsService? _sheets;
    private DriveService? _drive;

    public GoogleSheetsClient(IConfiguration config)
    {
        _keyPath = config["GoogleSheets:ServiceAccountKeyPath"];
        _folderId = config["GoogleSheets:SharedFolderId"];
    }

    private (SheetsService sheets, DriveService drive) GetServices()
    {
        if (_sheets is not null && _drive is not null)
            return (_sheets, _drive);

        if (string.IsNullOrEmpty(_keyPath) || !File.Exists(_keyPath))
            throw new InvalidOperationException(
                $"Google service account key not found at '{_keyPath}'. " +
                "Place a valid service-account.json file there to enable Sheets integration.");

        GoogleCredential credential;
        using (var stream = new FileStream(_keyPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(SheetsService.Scope.Spreadsheets, DriveService.Scope.Drive);
        }

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ContaZap"
        };

        _sheets = new SheetsService(initializer);
        _drive = new DriveService(initializer);
        return (_sheets, _drive);
    }

    public async Task<string> CreateSheetForUserAsync(DomainUser user, string templateId, CancellationToken ct = default)
    {
        var (_, driveService) = GetServices();

        // 1. Copy template
        var copyRequest = driveService.Files.Copy(new DriveData.File(), templateId);
        copyRequest.Fields = "id,name";
        var newFile = await copyRequest.ExecuteAsync(ct);

        // 2. Rename
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var safeName = user.Name.Replace(" ", "-").ToLowerInvariant();
        var newTitle = $"finance-{safeName}-{timestamp}";
        await driveService.Files.Update(new DriveData.File { Name = newTitle }, newFile.Id).ExecuteAsync(ct);

        // 3. Move to shared folder (optional)
        if (!string.IsNullOrEmpty(_folderId))
        {
            var moveRequest = driveService.Files.Update(new DriveData.File(), newFile.Id);
            moveRequest.AddParents = _folderId;
            await moveRequest.ExecuteAsync(ct);
        }

        // 4. Share with user email (optional)
        if (!string.IsNullOrEmpty(user.Email))
        {
            var permission = new DriveData.Permission { Type = "user", Role = "reader", EmailAddress = user.Email };
            await driveService.Permissions.Create(permission, newFile.Id).ExecuteAsync(ct);
        }

        return newFile.Id;
    }

    public async Task AppendTransactionAsync(
        string sheetId, string type, decimal amount,
        string category, string description, CancellationToken ct = default)
    {
        var (sheetsService, _) = GetServices();

        var range = "Transações!A:F";
        var row = new List<object>
        {
            DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
            type,
            amount.ToString("F2"),
            category,
            description,
            "WhatsApp"
        };

        var body = new ValueRange { Values = [row] };
        var request = sheetsService.Spreadsheets.Values.Append(body, sheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest
            .ValueInputOptionEnum.USERENTERED;
        await request.ExecuteAsync(ct);
    }

    public async Task<IList<IList<object>>> ReadSheetDataAsync(
        string sheetId, string range, CancellationToken ct = default)
    {
        var (sheetsService, _) = GetServices();
        var request = sheetsService.Spreadsheets.Values.Get(sheetId, range);
        var response = await request.ExecuteAsync(ct);
        return response.Values ?? [];
    }
}
