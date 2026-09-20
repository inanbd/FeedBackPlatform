using Dapper;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Infrastructure.Persistence.Repositories;

public sealed class FeedbackAppRepository(IDbConnectionFactory connectionFactory) : IFeedbackAppRepository
{
    private const string SelectColumns =
        "Id, OwnerUserId, Name, Description, RateLimitPerMinuteOverride, CreatedAtUtc";

    public async Task<FeedbackApp?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM FeedbackApps WHERE Id = @id", new { id }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<FeedbackApp>(command);
    }

    public async Task<IReadOnlyList<FeedbackApp>> ListByOwnerAsync(Guid ownerUserId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM FeedbackApps WHERE OwnerUserId = @ownerUserId ORDER BY CreatedAtUtc DESC",
            new { ownerUserId }, cancellationToken: ct);
        var rows = await connection.QueryAsync<FeedbackApp>(command);
        return rows.AsList();
    }

    public async Task<IReadOnlyList<FeedbackApp>> ListAllAsync(CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM FeedbackApps ORDER BY CreatedAtUtc DESC", cancellationToken: ct);
        var rows = await connection.QueryAsync<FeedbackApp>(command);
        return rows.AsList();
    }

    public async Task CreateAsync(FeedbackApp app, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            """
            INSERT INTO FeedbackApps (Id, OwnerUserId, Name, Description, RateLimitPerMinuteOverride, CreatedAtUtc)
            VALUES (@Id, @OwnerUserId, @Name, @Description, @RateLimitPerMinuteOverride, @CreatedAtUtc)
            """,
            app, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }

    public async Task SetRateLimitOverrideAsync(Guid id, int? requestsPerMinute, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "UPDATE FeedbackApps SET RateLimitPerMinuteOverride = @requestsPerMinute WHERE Id = @id",
            new { id, requestsPerMinute }, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }
}
