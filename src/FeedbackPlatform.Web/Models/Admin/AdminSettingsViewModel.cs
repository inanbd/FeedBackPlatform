using System.ComponentModel.DataAnnotations;

namespace FeedbackPlatform.Web.Models.Admin;

public sealed class AdminSettingsViewModel
{
    [Range(1, 100_000)]
    [Display(Name = "Global rate limit (requests per minute per API key)")]
    public int GlobalRateLimitPerMinute { get; set; }
}
