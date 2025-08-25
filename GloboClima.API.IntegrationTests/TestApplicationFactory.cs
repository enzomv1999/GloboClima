using System.Collections.Generic;
using System;
using GloboClima.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System.Linq;
using System.Threading.Tasks;

namespace GloboClima.API.IntegrationTests
{
    public class TestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            // Ensure JWT key is available very early via environment variable for Program.cs
            Environment.SetEnvironmentVariable("JWT_KEY", "integration_test_secret_key_1234567890");
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "integration_test_secret_key_1234567890"
                };
                configBuilder.AddInMemoryCollection(settings!);
            });

            builder.ConfigureServices(services =>
            {
                // Replace IAmazonDynamoDB with a client pointing to DynamoDB Local
                var toRemove = services.Where(d => d.ServiceType == typeof(IAmazonDynamoDB)).ToList();
                foreach (var d in toRemove)
                {
                    services.Remove(d);
                }

                var ddbUrl = System.Environment.GetEnvironmentVariable("DDB_LOCAL_URL") ?? "http://localhost:8000";
                var config = new AmazonDynamoDBConfig
                {
                    ServiceURL = ddbUrl,
                    UseHttp = true,
                    AuthenticationRegion = "us-east-2"
                };
                var client = new AmazonDynamoDBClient(new BasicAWSCredentials("dummy", "dummy"), config);
                services.AddSingleton<IAmazonDynamoDB>(client);

                // Ensure required tables exist in the local emulator (using the same client instance)
                CreateTableIfNotExistsAsync(client, "Users", "Id").GetAwaiter().GetResult();
                CreateTableIfNotExistsAsync(client, "Favorites", "Id").GetAwaiter().GetResult();
            });
        }

        private static async Task CreateTableIfNotExistsAsync(IAmazonDynamoDB client, string tableName, string hashKey)
        {
            var existing = await client.ListTablesAsync();
            if (existing.TableNames.Contains(tableName)) return;

            var request = new CreateTableRequest
            {
                TableName = tableName,
                BillingMode = BillingMode.PAY_PER_REQUEST,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition(hashKey, ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(hashKey, KeyType.HASH)
                }
            };

            try
            {
                await client.CreateTableAsync(request);
                await WaitForActiveAsync(client, tableName);
            }
            catch (ResourceInUseException)
            {
                // Table was created by another concurrent test factory; safe to proceed
            }
        }

        private static async Task WaitForActiveAsync(IAmazonDynamoDB client, string tableName)
        {
            for (int i = 0; i < 30; i++)
            {
                var desc = await client.DescribeTableAsync(tableName);
                if (desc.Table.TableStatus == TableStatus.ACTIVE)
                {
                    return;
                }
                await Task.Delay(500);
            }
        }
    }
}
