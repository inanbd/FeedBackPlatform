using System.Data;
using FeedbackPlatform.Application.Common.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FeedbackPlatform.Infrastructure.Persistence;

public sealed class DbConnectionFactory(IOptions<DatabaseOptions> options) : IDbConnectionFactory
{
    private readonly DatabaseOptions _options = options.Value;

    public IDbConnection CreateConnection()
    {
        IDbConnection connection = _options.Provider switch
        {
            DatabaseProvider.SqlServer => new SqlConnection(_options.ConnectionString),
            DatabaseProvider.Sqlite => new SqliteConnection(_options.ConnectionString),
            _ => throw new NotSupportedException($"Database provider '{_options.Provider}' is not supported.")
        };

        connection.Open();
        return connection;
    }
}
