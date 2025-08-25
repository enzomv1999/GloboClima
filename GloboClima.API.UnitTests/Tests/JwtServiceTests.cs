using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using FluentAssertions;
using GloboClima.API.Auth;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GloboClima.API.UnitTests.Tests
{
    public class JwtServiceTests
    {
        private JwtService CreateService(string key = "unit_test_secret_key_1234567890__")
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", key}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            return new JwtService(configuration);
        }

        [Fact]
        public void GenerateToken_ShouldContainNameClaim_AndValidExpiry()
        {
            var svc = CreateService();
            var token = svc.GenerateToken("alice");
            token.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type.EndsWith("/name")).Should().NotBeNull();
            jwt.Claims.First(c => c.Type == "unique_name" || c.Type.EndsWith("/name")).Value.Should().Be("alice");

            jwt.ValidTo.Should().BeAfter(DateTime.UtcNow.AddMinutes(59)).And.BeBefore(DateTime.UtcNow.AddHours(2));
        }
    }
}
