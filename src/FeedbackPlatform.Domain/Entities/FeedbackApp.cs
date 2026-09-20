namespace FeedbackPlatform.Domain.Entities;

/// <summary>A website or application registered by a user to collect feedback.</summary>
public sealed class FeedbackApp
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? RateLimitPerMinuteOverride { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
