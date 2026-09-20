using Dapper;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Application.Common.Models;
using FeedbackPlatform.Domain.Entities;
using Microsoft.Extensions.Options;

namespace FeedbackPlatform.Infrastructure.Persistence.Repositories;

public sealed class FeedbackRepository(IDbConnectionFactory connectionFactory, IOptions<DatabaseOptions> options)
    : IFeedbackRepository
{
    private const string SelectColumns =
        "Id, FeedbackAppId, AppVersion, Title, Comment, StarRating, ReporterName, ReporterContact, IpAddress, CustomFieldsJson, CreatedAtUtc";

    private readonly DatabaseProvider _provider = options.Value.Provider;

    public async Task<Guid> CreateAsync(Feedback feedback, CancellationToken ct = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            """
            INSERT INTO Feedbacks
                (Id, FeedbackAppId, AppVersion, Title, Comment, StarRating, ReporterName, ReporterContact, IpAddress, CustomFieldsJson, CreatedAtUtc)
            VALUES
                (@Id, @FeedbackAppId, @AppVersion, @Title, @Comment, @StarRating, @ReporterName, @ReporterContact, @IpAddress, @CustomFieldsJson, @CreatedAtUtc)
            """,
            feedback, cancellationToken: ct);
        await connection.ExecuteAsync(command);
        return feedback.Id;
    }

    public async Task<(IReadOnlyList<Feedback> Items, int TotalCount)> SearchAsync(
        FeedbackSearchQuery query, CancellationToken ct = default)
    {
        var (whereClause, parameters) = BuildAppScopeWhere(query.AppIds);

        if (query.MinStarRating is not null)
        {
            whereClause = AppendCondition(whereClause, "StarRating >= @minStarRating");
            parameters.Add("minStarRating", query.MinStarRating);
        }

        using var connection = connectionFactory.CreateConnection();

        var countCommand = new CommandDefinition(
            $"SELECT COUNT(1) FROM Feedbacks {whereClause}", parameters, cancellationToken: ct);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var offset = Math.Max(0, query.Page - 1) * query.PageSize;
        parameters.Add("pageSize", query.PageSize);
        parameters.Add("offset", offset);

        var pagingClause = _provider == DatabaseProvider.SqlServer
            ? "ORDER BY CreatedAtUtc DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY"
            : "ORDER BY CreatedAtUtc DESC LIMIT @pageSize OFFSET @offset";

        var itemsCommand = new CommandDefinition(
            $"SELECT {SelectColumns} FROM Feedbacks {whereClause} {pagingClause}", parameters, cancellationToken: ct);
        var items = await connection.QueryAsync<Feedback>(itemsCommand);

        return (items.AsList(), totalCount);
    }

    public async Task<IReadOnlyList<FeedbackDailyCount>> GetDailyCountsAsync(
        IReadOnlyCollection<Guid>? appIds, DateOnly fromDateUtc, DateOnly toDateUtc, CancellationToken ct = default)
    {
        var (whereClause, parameters) = BuildAppScopeWhere(appIds);

        var fromUtc = new DateTimeOffset(fromDateUtc.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toExclusiveUtc = new DateTimeOffset(toDateUtc.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        whereClause = AppendCondition(whereClause, "CreatedAtUtc >= @fromUtc AND CreatedAtUtc < @toExclusiveUtc");
        parameters.Add("fromUtc", fromUtc);
        parameters.Add("toExclusiveUtc", toExclusiveUtc);

        var dateExpression = _provider == DatabaseProvider.SqlServer
            ? "CAST(CreatedAtUtc AS DATE)"
            : "date(CreatedAtUtc)";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"""
             SELECT {dateExpression} AS FeedbackDate, COUNT(1) AS Count
             FROM Feedbacks
             {whereClause}
             GROUP BY {dateExpression}
             ORDER BY FeedbackDate
             """,
            parameters, cancellationToken: ct);

        var rows = await connection.QueryAsync<(string FeedbackDate, int Count)>(command);
        return rows
            .Select(r => new FeedbackDailyCount(DateOnly.Parse(r.FeedbackDate), r.Count))
            .ToList();
    }

    public async Task<IReadOnlyList<FeedbackRatingBucket>> GetRatingDistributionAsync(
        IReadOnlyCollection<Guid>? appIds, CancellationToken ct = default)
    {
        var (whereClause, parameters) = BuildAppScopeWhere(appIds);

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"""
             SELECT StarRating, COUNT(1) AS Count
             FROM Feedbacks
             {whereClause}
             GROUP BY StarRating
             ORDER BY StarRating
             """,
            parameters, cancellationToken: ct);

        // Read into a value tuple rather than the FeedbackRatingBucket record directly: Dapper matches
        // records against a constructor whose parameter types exactly match the reader's reported column
        // types, and SQLite reports ambiguous column metadata for aggregate columns when the result set
        // is empty, which throws instead of just returning zero rows. Tuple deserialization reads by
        // ordinal and converts, so it tolerates that.
        var rows = await connection.QueryAsync<(long StarRating, long Count)>(command);
        return rows.Select(r => new FeedbackRatingBucket((int)r.StarRating, (int)r.Count)).ToList();
    }

    public async Task<int> GetTotalCountAsync(IReadOnlyCollection<Guid>? appIds, CancellationToken ct = default)
    {
        var (whereClause, parameters) = BuildAppScopeWhere(appIds);

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            $"SELECT COUNT(1) FROM Feedbacks {whereClause}", parameters, cancellationToken: ct);
        return await connection.ExecuteScalarAsync<int>(command);
    }

    /// <summary>
    /// Builds the WHERE clause scoping results to a set of application ids.
    /// Null means no restriction (admin, all applications); an empty collection means
    /// "restrict to nothing" (a user who owns zero applications should see zero feedback).
    /// </summary>
    private static (string WhereClause, DynamicParameters Parameters) BuildAppScopeWhere(IReadOnlyCollection<Guid>? appIds)
    {
        var parameters = new DynamicParameters();

        if (appIds is null)
        {
            return (string.Empty, parameters);
        }

        if (appIds.Count == 0)
        {
            return ("WHERE 1 = 0", parameters);
        }

        parameters.Add("appIds", appIds);
        return ("WHERE FeedbackAppId IN @appIds", parameters);
    }

    private static string AppendCondition(string whereClause, string condition) =>
        string.IsNullOrEmpty(whereClause) ? $"WHERE {condition}" : $"{whereClause} AND {condition}";
}
