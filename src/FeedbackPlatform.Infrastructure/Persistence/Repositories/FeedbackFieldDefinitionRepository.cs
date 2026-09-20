using Dapper;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Infrastructure.Persistence.Repositories;

public sealed class FeedbackFieldDefinitionRepository(IDbConnectionFactory connectionFactory)
    : IFeedbackFieldDefinitionRepository
{
    private const string SelectColumns =
        "Id, FeedbackAppId, FieldKey, Label, FieldType, IsRequired, DisplayOrder, OptionsCsv";

    public async Task<IReadOnlyList<FeedbackFieldDefinition>> ListByAppAsync(Guid feedbackAppId, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM FeedbackFieldDefinitions WHERE FeedbackAppId = @feedbackAppId ORDER BY DisplayOrder",
            new { feedbackAppId }, cancellationToken: ct);
        var rows = await connection.QueryAsync<FeedbackFieldDefinition>(command);
        return rows.AsList();
    }

    public async Task<FeedbackFieldDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT {SelectColumns} FROM FeedbackFieldDefinitions WHERE Id = @id", new { id }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<FeedbackFieldDefinition>(command);
    }

    public async Task CreateAsync(FeedbackFieldDefinition definition, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            """
            INSERT INTO FeedbackFieldDefinitions (Id, FeedbackAppId, FieldKey, Label, FieldType, IsRequired, DisplayOrder, OptionsCsv)
            VALUES (@Id, @FeedbackAppId, @FieldKey, @Label, @FieldType, @IsRequired, @DisplayOrder, @OptionsCsv)
            """,
            definition, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "DELETE FROM FeedbackFieldDefinitions WHERE Id = @id", new { id }, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }
}
