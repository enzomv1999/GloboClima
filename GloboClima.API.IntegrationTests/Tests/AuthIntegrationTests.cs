using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GloboClima.API.IntegrationTests.Tests
{
    public class AuthIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly TestApplicationFactory _factory;
        public AuthIntegrationTests(TestApplicationFactory factory)
        {
            _factory = factory;
        }

        private static HttpClient CreateClientWithHttpsProto(TestApplicationFactory factory)
        {
            var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
            return client;
        }

        [Fact]
        public async Task Register_Then_Login_Should_Return_Token()
        {
            using var client = CreateClientWithHttpsProto(_factory);

            var username = $"user_{Guid.NewGuid():N}";
            var password = "secret123";

            var regContent = new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");
            var regResponse = await client.PostAsync("/api/auth/register", regContent);
            regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginContent = new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await loginResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.TryGetProperty("token", out var tokenProp).Should().BeTrue();
            tokenProp.GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_With_WrongPassword_Should_Return_Unauthorized()
        {
            using var client = CreateClientWithHttpsProto(_factory);

            var username = $"user_{Guid.NewGuid():N}";
            var reg = new StringContent(JsonSerializer.Serialize(new { username, password = "secret123" }), Encoding.UTF8, "application/json");
            var regResponse = await client.PostAsync("/api/auth/register", reg);
            regResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var badLogin = new StringContent(JsonSerializer.Serialize(new { username, password = "wrongpwd" }), Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("/api/auth/login", badLogin);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
