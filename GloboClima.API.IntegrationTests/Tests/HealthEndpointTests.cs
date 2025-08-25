using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GloboClima.API.IntegrationTests.Tests
{
    public class HealthEndpointTests : IClassFixture<TestApplicationFactory>
    {
        private readonly TestApplicationFactory _factory;
        public HealthEndpointTests(TestApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Api_Health_Should_Return_Success()
        {
            using var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync("/api/health");

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.TemporaryRedirect);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var text = await response.Content.ReadAsStringAsync();
                text.Should().Contain("Healthy");
            }
        }
    }
}
