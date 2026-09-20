using System.ComponentModel.DataAnnotations;

namespace FeedbackPlatform.Web.Models.Applications;

public sealed class CreateFeedbackAppViewModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
