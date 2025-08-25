using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using GloboClima.API.Models;
using GloboClima.API.Utils;

namespace GloboClima.API.Services
{
    public class UserService
    {
        private readonly IDynamoDBContext _context;

        public UserService(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            var users = await _context.ScanAsync<User>(new[]
            {
                new ScanCondition("Username", ScanOperator.Equal, username)
            }).GetRemainingAsync();

            return users.FirstOrDefault();
        }

        public async Task<(bool Success, string Error)> RegisterAsync(string username, string password)
        {
            var existing = await GetByUsernameAsync(username);
            if (existing != null)
                return (false, "Usuário já existe.");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = PasswordHasher.Hash(password),
                CreatedAt = DateTime.UtcNow
            };

            await _context.SaveAsync(user);
            return (true, null);
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return null;

            var valid = PasswordHasher.Verify(password, user.PasswordHash);
            return valid ? user : null;
        }
    }
}
