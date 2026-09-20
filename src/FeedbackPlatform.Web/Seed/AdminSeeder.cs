using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Domain.Entities;
using FeedbackPlatform.Domain.Enums;

namespace FeedbackPlatform.Web.Seed;

/// <summary>
/// Creates the first admin account from configuration ("Seed:AdminEmail" / "Seed:AdminPassword") so
/// there is always a way to log in as an administrator without a dedicated admin-creation UI.
/// A no-op once that account (or any account with that email) already exists.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var userRepository = services.GetRequiredService<IUserRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();
        if (await userRepository.EmailExistsAsync(normalizedEmail))
        {
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = configuration["Seed:AdminDisplayName"] ?? "Administrator",
            PasswordHash = passwordHasher.Hash(adminPassword),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await userRepository.CreateAsync(admin);
    }
}
