using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;

namespace GloboClima.API.Models
{
    [DynamoDBTable("Users")]
    public class User
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
