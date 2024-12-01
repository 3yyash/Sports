using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Location Location { get; set; }
    public int SkillLevel { get; set; } = 5;
    public List<string> Sports { get; set; }
    public double AverageRating { get; set; } = 0;
    public int TotalGamesPlayed { get; set; } = 0;
}

public class Location
{
    public string Type { get; set; } = "Point";
    public double[] Coordinates { get; set; } // [longitude, latitude]
}
