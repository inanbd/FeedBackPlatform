namespace FeedbackPlatform.Domain.Entities;

public sealed class Feedback
{
    public Guid Id { get; set; }
    public Guid FeedbackAppId { get; set; }
    public required string AppVersion { get; set; }
    public required string Title { get; set; }
    public required string Comment { get; set; }
    public int StarRating { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterContact { get; set; }
    public required string IpAddress { get; set; }

    /// <summary>Serialized JSON object holding values for the application's custom field definitions.</summary>
    public string? CustomFieldsJson { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
