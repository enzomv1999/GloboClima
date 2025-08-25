using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GloboClima.API.IntegrationTests.Tests
{
    public class FavoritesIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly TestApplicationFactory _factory;
        public FavoritesIntegrationTests(TestApplicationFactory factory)
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

        private static async Task<string> RegisterAndLoginAsync(HttpClient client)
        {
            var username = $"fav_{Guid.NewGuid():N}";
            var password = "secret123";

            var regContent = new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");
            var regResp = await client.PostAsync("/api/auth/register", regContent);
            regResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginContent = new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");
            var loginResp = await client.PostAsync("/api/auth/login", loginContent);
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginJson = await loginResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(loginJson);
            var token = doc.RootElement.GetProperty("token").GetString();
            token.Should().NotBeNullOrWhiteSpace();
            return token!;
        }

        [Fact]
        public async Task Favorite_CRUD_EndToEnd()
        {
            using var client = CreateClientWithHttpsProto(_factory);
            var token = await RegisterAndLoginAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create
            var favContent = new StringContent(JsonSerializer.Serialize(new { type = "city", name = "Sao Paulo" }), Encoding.UTF8, "application/json");
            var createResp = await client.PostAsync("/api/favorite", favContent);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdJson = await createResp.Content.ReadAsStringAsync();
            using var createdDoc = JsonDocument.Parse(createdJson);
            var id = createdDoc.RootElement.GetProperty("id").GetString();
            id.Should().NotBeNullOrWhiteSpace();

            // List
            var listResp = await client.GetAsync("/api/favorite");
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var listJson = await listResp.Content.ReadAsStringAsync();
            using var listDoc = JsonDocument.Parse(listJson);
            listDoc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
            listDoc.RootElement.EnumerateArray().Any(e => e.GetProperty("id").GetString() == id).Should().BeTrue();

            // Delete
            var delResp = await client.DeleteAsync($"/api/favorite/{id}");
            delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // List again
            var list2Resp = await client.GetAsync("/api/favorite");
            list2Resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var list2Json = await list2Resp.Content.ReadAsStringAsync();
            using var list2Doc = JsonDocument.Parse(list2Json);
            list2Doc.RootElement.EnumerateArray().Any(e => e.GetProperty("id").GetString() == id).Should().BeFalse();
        }
    }
}
