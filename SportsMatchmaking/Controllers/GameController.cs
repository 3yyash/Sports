using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsMatchmaking.Models;
using SportsMatchmaking.Services;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGame(CreateGameRequest model)
    {
        var userId = User.Claims.First(c => c.Type == "id").Value;
        var game = await _gameService.CreateGame(model, userId);

        if (game == null)
            return BadRequest(new { message = "Game creation failed" });

        return Ok(game);
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> FindGames([FromQuery] GameSearchRequest model)
    {
        var games = await _gameService.SearchGames(model);

        return Ok(games);
    }

    [HttpPost("{gameId}/join")]
    [Authorize]
    public async Task<IActionResult> JoinGame(string gameId)
    {
        var userId = User.Claims.First(c => c.Type == "id").Value;
        var result = await _gameService.JoinGame(gameId, userId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    [HttpPost("{gameId}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveGame(string gameId)
    {
        var userId = User.Claims.First(c => c.Type == "id").Value;
        var result = await _gameService.LeaveGame(gameId, userId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }
}
