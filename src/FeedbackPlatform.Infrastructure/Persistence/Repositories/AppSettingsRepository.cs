using Dapper;
using FeedbackPlatform.Application.Common.Interfaces;

namespace FeedbackPlatform.Infrastructure.Persistence.Repositories;

public sealed class AppSettingsRepository(IDbConnectionFactory connectionFactory) : IAppSettingsRepository
{
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT Value FROM AppSettings WHERE [Key] = @key", new { key }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<string>(command);
    }

    public async Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var existing = await GetAsync(key, ct);
        var sql = existing is null
            ? "INSERT INTO AppSettings ([Key], [Value]) VALUES (@key, @value)"
            : "UPDATE AppSettings SET [Value] = @value WHERE [Key] = @key";

        var command = new CommandDefinition(sql, new { key, value }, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }
}
