using System.ComponentModel.DataAnnotations;
using FeedbackPlatform.Domain.Enums;

namespace FeedbackPlatform.Web.Models.Applications;

public sealed class CreateCustomFieldViewModel
{
    [Required, StringLength(100)]
    [RegularExpression("^[a-z][a-z0-9_]*$", ErrorMessage = "Use lowercase letters, numbers and underscores, starting with a letter.")]
    public string FieldKey { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Label { get; set; } = string.Empty;

    public FeedbackFieldType FieldType { get; set; } = FeedbackFieldType.Text;

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    [StringLength(1000)]
    public string? OptionsCsv { get; set; }
}
