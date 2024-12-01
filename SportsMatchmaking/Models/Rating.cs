public class Rating
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string GameId { get; set; }
    public string RaterId { get; set; }
    public string RatedId { get; set; }
    public int RatingValue { get; set; }
    public string Comment { get; set; }
}
