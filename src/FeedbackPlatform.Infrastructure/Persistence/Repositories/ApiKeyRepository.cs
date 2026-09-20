using Dapper;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Infrastructure.Persistence.Repositories;

public sealed class ApiKeyRepository(IDbConnectionFactory connectionFactory) : IApiKeyRepository
{
    private const string SelectColumns =
        "Id, FeedbackAppId, KeyPrefix, KeyHash, CreatedAtUtc, RevokedAtUtc, LastUsedAtUtc";

    public async Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM ApiKeys WHERE Id = @id", new { id }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<ApiKey>(command);
    }

    public async Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM ApiKeys WHERE KeyHash = @keyHash", new { keyHash }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<ApiKey>(command);
    }

    public async Task<IReadOnlyList<ApiKey>> ListByAppAsync(Guid feedbackAppId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM ApiKeys WHERE FeedbackAppId = @feedbackAppId ORDER BY CreatedAtUtc DESC",
            new { feedbackAppId }, cancellationToken: ct);
        var rows = await connection.QueryAsync<ApiKey>(command);
        return rows.AsList();
    }

    public async Task CreateAsync(ApiKey apiKey, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            """
            INSERT INTO ApiKeys (Id, FeedbackAppId, KeyPrefix, KeyHash, CreatedAtUtc, RevokedAtUtc, LastUsedAtUtc)
            VALUES (@Id, @FeedbackAppId, @KeyPrefix, @KeyHash, @CreatedAtUtc, @RevokedAtUtc, @LastUsedAtUtc)
            """,
            apiKey, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }

    public async Task RevokeAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "UPDATE ApiKeys SET RevokedAtUtc = @now WHERE Id = @id",
            new { id, now = DateTimeOffset.UtcNow }, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }

    public async Task UpdateLastUsedAsync(Guid id, DateTimeOffset whenUtc, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "UPDATE ApiKeys SET LastUsedAtUtc = @whenUtc WHERE Id = @id",
            new { id, whenUtc }, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }
}
