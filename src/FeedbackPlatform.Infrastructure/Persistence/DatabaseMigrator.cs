using System.Reflection;
using FeedbackPlatform.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace FeedbackPlatform.Infrastructure.Persistence;

/// <summary>
/// Applies the base schema on startup. There is no versioned migration history here by design
/// (no EF Core) — every statement in the scripts is written to be safely re-runnable
/// (CREATE TABLE/INDEX IF NOT EXISTS, or the SQL Server OBJECT_ID guard equivalent).
/// </summary>
public sealed class DatabaseMigrator(IDbConnectionFactory connectionFactory, IOptions<DatabaseOptions> options)
{
    private readonly DatabaseOptions _options = options.Value;

    public void Migrate()
    {
        var scriptName = _options.Provider switch
        {
            DatabaseProvider.SqlServer => "schema.sqlserver.sql",
            DatabaseProvider.Sqlite => "schema.sqlite.sql",
            _ => throw new NotSupportedException($"Database provider '{_options.Provider}' is not supported.")
        };

        var script = ReadEmbeddedScript(scriptName);

        using var connection = connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = script;
        command.ExecuteNonQuery();
    }

    private static string ReadEmbeddedScript(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"FeedbackPlatform.Infrastructure.Persistence.Scripts.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded schema script '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
