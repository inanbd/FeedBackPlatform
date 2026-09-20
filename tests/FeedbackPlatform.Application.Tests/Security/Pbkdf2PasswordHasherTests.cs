using FeedbackPlatform.Infrastructure.Security;
using Xunit;

namespace FeedbackPlatform.Application.Tests.Security;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("correct-password");

        Assert.True(_hasher.Verify("correct-password", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correct-password");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentOutputForSamePasswordDueToRandomSalt()
    {
        var hash1 = _hasher.Hash("same-password");
        var hash2 = _hasher.Hash("same-password");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.Verify("same-password", hash1));
        Assert.True(_hasher.Verify("same-password", hash2));
    }
}
