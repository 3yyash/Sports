using SportsMatchmaking.Models;
using SportsMatchmaking.Data;
using MongoDB.Driver;

namespace SportsMatchmaking.Services
{
    public class GameService
    {
        private readonly IMongoCollection<Game> _gamesCollection;

        public GameService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("SportsMatchmakingDB");
            _gamesCollection = database.GetCollection<Game>("Games");
        }

        public async Task<Game> CreateGameAsync(Game game)
        {
            await _gamesCollection.InsertOneAsync(game);
            return game;
        }

        public async Task<List<Game>> FindGamesAsync(double[] coordinates, double maxDistance, string sport, int? skillLevel)
        {
            var filter = Builders<Game>.Filter.And(
                Builders<Game>.Filter.Eq(g => g.Status, "open"),
                Builders<Game>.Filter.Gt(g => g.DateTime, DateTime.UtcNow)
            );

            if (!string.IsNullOrEmpty(sport))
            {
                filter = Builders<Game>.Filter.And(filter, Builders<Game>.Filter.Eq(g => g.Sport, sport));
            }

            if (skillLevel.HasValue)
            {
                filter = Builders<Game>.Filter.And(filter, Builders<Game>.Filter.Gte(g => g.SkillLevel, skillLevel.Value - 2));
                filter = Builders<Game>.Filter.And(filter, Builders<Game>.Filter.Lte(g => g.SkillLevel, skillLevel.Value + 2));
            }

            if (coordinates != null && coordinates.Length == 2)
            {
                filter = Builders<Game>.Filter.And(filter,
                    Builders<Game>.Filter.NearSphere(
                        g => g.Location.Coordinates,
                        coordinates[0],
                        coordinates[1],
                        maxDistance
                    )
                );
            }

            return await _gamesCollection.Find(filter).ToListAsync();
        }

        public async Task<bool> JoinGameAsync(string gameId, string userId)
        {
            var game = await _gamesCollection.Find(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null || game.CurrentPlayers.Contains(userId) || game.CurrentPlayers.Count >= game.RequiredPlayers)
                return false;

            var update = Builders<Game>.Update.AddToSet(g => g.CurrentPlayers, userId);
            if (game.CurrentPlayers.Count + 1 == game.RequiredPlayers)
            {
                update = Builders<Game>.Update.Combine(update, Builders<Game>.Update.Set(g => g.Status, "full"));
            }

            var result = await _gamesCollection.UpdateOneAsync(g => g.Id == gameId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> LeaveGameAsync(string gameId, string userId)
        {
            var game = await _gamesCollection.Find(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null || !game.CurrentPlayers.Contains(userId))
                return false;

            var update = Builders<Game>.Update.Pull(g => g.CurrentPlayers, userId);
            if (game.Status == "full")
            {
                update = Builders<Game>.Update.Combine(update, Builders<Game>.Update.Set(g => g.Status, "open"));
            }

            var result = await _gamesCollection.UpdateOneAsync(g => g.Id == gameId, update);
            return result.ModifiedCount > 0;
        }
    }
}
