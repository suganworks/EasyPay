using EasyPay.Core.Entities;
using System.Security.Claims;

namespace EasyPay.Core.Interfaces.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    int? GetUserIdFromToken(string token);
}
