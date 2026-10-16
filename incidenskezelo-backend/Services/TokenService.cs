using System.Security.Claims;
using System.Text;
using incidenskezelo_backend.Auth;
using incidenskezelo_backend.Configuration;
using incidenskezelo_backend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace incidenskezelo_backend.Services;

public class TokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }


    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
    {
        var expiresAtUtc = _timeProvider.GetUtcNow().UtcDateTime.AddHours(_options.ExpireHours);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAtUtc,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(TokenClaims.UserId, user.UserId.ToString()),
                new Claim(TokenClaims.Name, user.Name),
                new Claim(TokenClaims.Email, user.Email),
                new Claim(TokenClaims.Role, user.Role.ToString()),
            ]),
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAtUtc);
    }
}
