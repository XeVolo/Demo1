using System;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Authenticator;
using BCrypt.Net;
using GrpcTestServer;
using GrpcTestServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _config;

    public GreeterService(ApplicationDbContext dbContext, IConfiguration config)
    {
        _dbContext = dbContext;
        _config = config;
    }

    public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            throw new RpcException(new Status(StatusCode.AlreadyExists, "User already exists"));

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse { Token = GenerateJwtToken(user.Email) };
    }

    public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentials"));

        return new AuthResponse { Token = GenerateJwtToken(user.Email) };
    }

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
