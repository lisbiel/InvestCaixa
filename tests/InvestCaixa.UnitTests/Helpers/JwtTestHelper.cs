using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvestCaixa.UnitTests.Helpers;

public static class JwtTestHelper
{
    private const string SECRET_KEY = "super-secret-key-for-tests-12345678901234567890"; // Mínimo 32 caracteres
    private const string ISSUER = "InvestCaixa-Test";
    private const string AUDIENCE = "InvestCaixa-Test-Users";

    public static string GenerateTestToken(int userId = 1, string userName = "test@test.com", int expirationMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(SECRET_KEY);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, userName),
                new Claim("userId", userId.ToString()),
                new Claim("userName", userName)
            }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = ISSUER,
            Audience = AUDIENCE,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static TokenValidationParameters GetTestTokenValidationParameters()
    {
        var key = Encoding.UTF8.GetBytes(SECRET_KEY);
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = ISSUER,
            ValidAudience = AUDIENCE,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    }
}