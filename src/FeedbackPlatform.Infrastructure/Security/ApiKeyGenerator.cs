using System.Security.Cryptography;
using System.Text;
using FeedbackPlatform.Application.Common.Interfaces;

namespace FeedbackPlatform.Infrastructure.Security;

/// <summary>
/// Generates opaque API keys of the form "fbk_&lt;44 url-safe base64 chars&gt;". Only the
/// SHA-256 hash of the full key is ever persisted; the raw value is shown to the user once,
/// at creation time.
/// </summary>
public sealed class ApiKeyGenerator : IApiKeyGenerator
{
    private const string Prefix = "fbk_";
    private const int SecretSizeBytes = 32;

    public GeneratedApiKey Generate()
    {
        var secretBytes = RandomNumberGenerator.GetBytes(SecretSizeBytes);
        var secret = Convert.ToBase64String(secretBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var rawKey = $"{Prefix}{secret}";
        var displayPrefix = rawKey[..Math.Min(rawKey.Length, 12)];

        return new GeneratedApiKey(rawKey, displayPrefix, Hash(rawKey));
    }

    public string Hash(string rawKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
