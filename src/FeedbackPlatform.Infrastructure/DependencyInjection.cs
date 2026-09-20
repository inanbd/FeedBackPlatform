using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Infrastructure.Email;
using FeedbackPlatform.Infrastructure.Persistence;
using FeedbackPlatform.Infrastructure.Persistence.Repositories;
using FeedbackPlatform.Infrastructure.RateLimiting;
using FeedbackPlatform.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FeedbackPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();
        if (databaseOptions.Provider == DatabaseProvider.Sqlite)
        {
            DapperSqliteSupport.RegisterSqliteTypeHandlers();
        }

        services.AddMemoryCache();

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<DatabaseMigrator>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFeedbackAppRepository, FeedbackAppRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IFeedbackFieldDefinitionRepository, FeedbackFieldDefinitionRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IApiKeyGenerator, ApiKeyGenerator>();
        services.AddScoped<IRateLimitSettingsProvider, RateLimitSettingsProvider>();

        services.AddSingleton<EmailNotificationQueue>();
        services.AddSingleton<IEmailNotificationQueue>(sp => sp.GetRequiredService<EmailNotificationQueue>());
        services.AddHostedService<EmailNotificationBackgroundService>();

        return services;
    }
}
