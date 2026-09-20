using FeedbackPlatform.Domain.Enums;

namespace FeedbackPlatform.Domain.Entities;

/// <summary>A custom field an application owner has added to their feedback form.</summary>
public sealed class FeedbackFieldDefinition
{
    public Guid Id { get; set; }
    public Guid FeedbackAppId { get; set; }

    /// <summary>Machine key used inside the feedback's CustomFieldsJson, e.g. "browser_name".</summary>
    public required string FieldKey { get; set; }

    public required string Label { get; set; }
    public FeedbackFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>Comma-separated options, only used when FieldType is Select.</summary>
    public string? OptionsCsv { get; set; }
}
