namespace AlphaApi.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = PasswordHasher.Hash("mypassword");
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSameInput()
    {
        var hash1 = PasswordHasher.Hash("mypassword");
        var hash2 = PasswordHasher.Hash("mypassword");
        Assert.NotEqual(hash1, hash2); // BCrypt uses a random salt per call
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var hash = PasswordHasher.Hash("mypassword");
        Assert.True(PasswordHasher.Verify("mypassword", hash));
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hash = PasswordHasher.Hash("mypassword");
        Assert.False(PasswordHasher.Verify("wrongpassword", hash));
    }
}
