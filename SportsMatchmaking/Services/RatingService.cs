using SportsMatchmaking.Models;
using SportsMatchmaking.Data;
using MongoDB.Driver;

namespace SportsMatchmaking.Services
{
    public class RatingService
    {
        private readonly IMongoCollection<Rating> _ratingsCollection;
        private readonly IMongoCollection<Game> _gamesCollection;
        private readonly IMongoCollection<User> _usersCollection;

        public RatingService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("SportsMatchmakingDB");
            _ratingsCollection = database.GetCollection<Rating>("Ratings");
            _gamesCollection = database.GetCollection<Game>("Games");
            _usersCollection = database.GetCollection<User>("Users");
        }

        public async Task<Rating> RatePlayerAsync(string gameId, string raterId, string ratedUserId, int ratingValue, string? comment)
        {
            var game = await _gamesCollection.Find(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null || !game.CurrentPlayers.Contains(raterId) || !game.CurrentPlayers.Contains(ratedUserId))
            {
                throw new Exception("Invalid game or players.");
            }

            var existingRating = await _ratingsCollection.Find(r =>
                r.GameId == gameId && r.RaterId == raterId && r.RatedUserId == ratedUserId
            ).FirstOrDefaultAsync();

            if (existingRating != null)
            {
                throw new Exception("Player already rated for this game.");
            }

            var newRating = new Rating
            {
                GameId = gameId,
                RaterId = raterId,
                RatedUserId = ratedUserId,
                RatingValue = ratingValue,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            await _ratingsCollection.InsertOneAsync(newRating);

            await UpdateUserRatingsAsync(ratedUserId);

            return newRating;
        }

        public async Task<List<Rating>> GetUserRatingsAsync(string userId)
        {
            return await _ratingsCollection
                .Find(r => r.RatedUserId == userId)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        private async Task UpdateUserRatingsAsync(string userId)
        {
            var userRatings = await _ratingsCollection.Find(r => r.RatedUserId == userId).ToListAsync();
            var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            if (user != null)
            {
                user.AverageRating = userRatings.Average(r => r.RatingValue);
                user.TotalGamesPlayed = await _gamesCollection.CountDocumentsAsync(g =>
                    g.CurrentPlayers.Contains(userId) && g.Status == "completed");

                var update = Builders<User>.Update
                    .Set(u => u.AverageRating, user.AverageRating)
                    .Set(u => u.TotalGamesPlayed, user.TotalGamesPlayed);

                await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);
            }
        }
    }
}
