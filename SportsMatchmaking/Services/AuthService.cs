using Microsoft.AspNetCore.Identity.Data;
using MongoDB.Driver;
using SportsMatchmaking.Models;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly string _jwtSecret;

    public AuthService(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase("sports-matchmaking");
        _users = database.GetCollection<User>("Users");
        _jwtSecret = configuration.GetValue<string>("AppSettings:JwtSecret");
    }

    public async Task<AuthResponse> Register(RegisterRequest model)
    {
        // Check if user exists
        var existingUser = await _users.Find(u => u.Email == model.Email || u.Username == model.Username).FirstOrDefaultAsync();
        if (existingUser != null)
            return new AuthResponse { Success = false, Message = "User already exists" };

        // Create new user
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Location = new Location
            {
                Coordinates = model.Location
            },
            Sports = model.Sports
        };

        await _users.InsertOneAsync(user);

        var token = GenerateJwtToken(user.Id);

        return new AuthResponse { Success = true, Token = token };
    }

    public async Task<AuthResponse> Login(LoginRequest model)
    {
        var user = await _users.Find(u => u.Email == model.Email).FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            return new AuthResponse { Success = false, Message = "Invalid credentials" };

        var token = GenerateJwtToken(user.Id);

        return new AuthResponse { Success = true, Token = token };
    }

    private string GenerateJwtToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", userId) }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<User> GetUserById(string userId)
    {
        return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }
}
