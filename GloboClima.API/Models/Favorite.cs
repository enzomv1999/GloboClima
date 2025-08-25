using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.API.Models
{
    [DynamoDBTable("Favorites")] 
    public class Favorite
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        public string Username { get; set; } 
        public string Type { get; set; } 
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
