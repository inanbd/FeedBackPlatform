using FeedbackPlatform.Infrastructure.Security;
using Xunit;

namespace FeedbackPlatform.Application.Tests.Security;

public class ApiKeyGeneratorTests
{
    private readonly ApiKeyGenerator _generator = new();

    [Fact]
    public void Generate_ProducesKeyWhoseHashMatchesRecomputedHash()
    {
        var generated = _generator.Generate();

        Assert.Equal(generated.KeyHash, _generator.Hash(generated.RawKey));
    }

    [Fact]
    public void Generate_ProducesUniqueKeysEachTime()
    {
        var first = _generator.Generate();
        var second = _generator.Generate();

        Assert.NotEqual(first.RawKey, second.RawKey);
        Assert.NotEqual(first.KeyHash, second.KeyHash);
    }

    [Fact]
    public void Hash_IsDeterministicForTheSameInput()
    {
        const string rawKey = "fbk_some-fixed-value";

        Assert.Equal(_generator.Hash(rawKey), _generator.Hash(rawKey));
    }
}
