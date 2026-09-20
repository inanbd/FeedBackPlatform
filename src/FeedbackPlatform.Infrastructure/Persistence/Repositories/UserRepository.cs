using Dapper;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;

namespace FeedbackPlatform.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT Id, Email, DisplayName, PasswordHash, Role, IsActive, CreatedAtUtc FROM Users WHERE Id = @id",
            new { id }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT Id, Email, DisplayName, PasswordHash, Role, IsActive, CreatedAtUtc FROM Users WHERE Email = @email",
            new { email }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "SELECT COUNT(1) FROM Users WHERE Email = @email", new { email }, cancellationToken: ct);
        return await connection.ExecuteScalarAsync<int>(command) > 0;
    }

    public async Task CreateAsync(User user, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            """
            INSERT INTO Users (Id, Email, DisplayName, PasswordHash, Role, IsActive, CreatedAtUtc)
            VALUES (@Id, @Email, @DisplayName, @PasswordHash, @Role, @IsActive, @CreatedAtUtc)
            """,
            user, cancellationToken: ct);
        await connection.ExecuteAsync(command);
    }
}
