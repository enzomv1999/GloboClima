using FluentAssertions;
using GloboClima.API.Utils;
using Xunit;

namespace GloboClima.API.UnitTests.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_Should_Be_Deterministic_And_Verify_Should_Work()
        {
            var password = "MyS3cret!";
            var hash1 = PasswordHasher.Hash(password);
            var hash2 = PasswordHasher.Hash(password);

            hash1.Should().NotBeNullOrEmpty();
            hash1.Should().Be(hash2);

            PasswordHasher.Verify(password, hash1).Should().BeTrue();
            PasswordHasher.Verify("wrong", hash1).Should().BeFalse();
        }
    }
}
