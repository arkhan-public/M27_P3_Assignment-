using QAWebApp.Models;

namespace QAWebApp.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    int? ValidateToken(string token);
}
