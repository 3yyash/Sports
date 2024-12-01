public class Game
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Creator { get; set; }
    public string Sport { get; set; }
    public Location Location { get; set; }
    public DateTime Datetime { get; set; }
    public int RequiredPlayers { get; set; }
    public List<string> CurrentPlayers { get; set; }
    public int SkillLevel { get; set; }
    public string Status { get; set; } = "open";
    public bool CourtBooked { get; set; } = false;
}
