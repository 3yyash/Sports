using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsMatchmaking.Models;
using SportsMatchmaking.Services;

[ApiController]
[Route("api/[controller]")]
public class RatingController : ControllerBase
{
    private readonly RatingService _ratingService;

    public RatingController(RatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RatePlayer(RatingRequest model)
    {
        var raterId = User.Claims.First(c => c.Type == "id").Value;
        var result = await _ratingService.RatePlayer(model, raterId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserRatings(string userId)
    {
        var ratings = await _ratingService.GetRatingsByUser(userId);

        return Ok(ratings);
    }
}
