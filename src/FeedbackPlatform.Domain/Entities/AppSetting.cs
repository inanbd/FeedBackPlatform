namespace FeedbackPlatform.Domain.Entities;

/// <summary>Simple key/value store for admin-configurable platform settings (e.g. default rate limit).</summary>
public sealed class AppSetting
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}

public static class AppSettingKeys
{
    public const string DefaultRateLimitPerMinute = "Api:DefaultRateLimitPerMinute";
}
