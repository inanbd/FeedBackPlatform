using System.Security.Claims;
using FeedbackPlatform.Application.Features.Auth.Queries;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FeedbackPlatform.Web.Auth;

public static class PortalClaimsFactory
{
    public static ClaimsPrincipal CreatePrincipal(LoginResult login)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, login.UserId.ToString()),
            new Claim(ClaimTypes.Email, login.Email),
            new Claim(ClaimTypes.Name, login.DisplayName),
            new Claim(ClaimTypes.Role, login.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
