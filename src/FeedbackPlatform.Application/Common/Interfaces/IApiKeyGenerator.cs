namespace FeedbackPlatform.Application.Common.Interfaces;

public sealed record GeneratedApiKey(string RawKey, string KeyPrefix, string KeyHash);

public interface IApiKeyGenerator
{
    GeneratedApiKey Generate();
    string Hash(string rawKey);
}
