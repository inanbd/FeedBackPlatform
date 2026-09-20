using System.Data;
using System.Globalization;
using Dapper;

namespace FeedbackPlatform.Infrastructure.Persistence;

/// <summary>
/// SQLite has no native GUID/DATETIMEOFFSET type; our schema stores both as TEXT.
/// These handlers make Dapper round-trip them as strings when running against SQLite.
/// SQL Server has native UNIQUEIDENTIFIER/DATETIMEOFFSET types, so it never needs these.
/// DateTimeOffset values are always normalized to UTC and stored in SQLite's canonical
/// "YYYY-MM-DD HH:MM:SS.SSS" format so that SQLite's built-in date()/strftime() functions
/// (used for dashboard grouping) and plain string ordering both work correctly.
/// </summary>
internal sealed class GuidStringTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();
    public override Guid Parse(object value) => value is Guid g ? g : Guid.Parse((string)value);
}

internal sealed class NullableGuidStringTypeHandler : SqlMapper.TypeHandler<Guid?>
{
    public override void SetValue(IDbDataParameter parameter, Guid? value) =>
        parameter.Value = value is null ? DBNull.Value : value.Value.ToString();

    public override Guid? Parse(object value) =>
        value is null or DBNull ? null : value is Guid g ? g : Guid.Parse((string)value);
}

internal sealed class DateTimeOffsetStringTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    private const string Format = "yyyy-MM-dd HH:mm:ss.fff";

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value) =>
        parameter.Value = value.UtcDateTime.ToString(Format, CultureInfo.InvariantCulture);

    public override DateTimeOffset Parse(object value)
    {
        if (value is DateTimeOffset dto) return dto;
        var dt = DateTime.Parse((string)value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        return new DateTimeOffset(dt, TimeSpan.Zero);
    }
}

internal sealed class NullableDateTimeOffsetStringTypeHandler : SqlMapper.TypeHandler<DateTimeOffset?>
{
    private const string Format = "yyyy-MM-dd HH:mm:ss.fff";

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value) =>
        parameter.Value = value is null ? DBNull.Value : value.Value.UtcDateTime.ToString(Format, CultureInfo.InvariantCulture);

    public override DateTimeOffset? Parse(object value)
    {
        if (value is null or DBNull) return null;
        if (value is DateTimeOffset dto) return dto;
        var dt = DateTime.Parse((string)value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        return new DateTimeOffset(dt, TimeSpan.Zero);
    }
}

public static class DapperSqliteSupport
{
    public static void RegisterSqliteTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new GuidStringTypeHandler());
        SqlMapper.AddTypeHandler(new NullableGuidStringTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetStringTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeOffsetStringTypeHandler());
    }
}
