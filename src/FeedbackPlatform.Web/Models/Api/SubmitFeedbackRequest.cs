namespace FeedbackPlatform.Web.Models.Api;

public sealed class SubmitFeedbackRequest
{
    public required string AppVersion { get; set; }
    public required string Title { get; set; }
    public required string Comment { get; set; }
    public required int StarRating { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterContact { get; set; }
    public Dictionary<string, string>? CustomFields { get; set; }
}
