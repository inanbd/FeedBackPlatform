namespace FeedbackPlatform.Domain.Entities;

public sealed class ApiKey
{
    public Guid Id { get; set; }
    public Guid FeedbackAppId { get; set; }

    /// <summary>Short, non-secret prefix shown in the UI to identify the key (e.g. "fbk_ab12cd34").</summary>
    public required string KeyPrefix { get; set; }

    /// <summary>SHA-256 hash of the full secret. The raw secret is never stored.</summary>
    public required string KeyHash { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public bool IsActive => RevokedAtUtc is null;
}
