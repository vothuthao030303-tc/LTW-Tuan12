using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RealEstateWeb.Data;

namespace RealEstateWeb.Services;

public class CurrentUserCookieEvents : CookieAuthenticationEvents
{
    private readonly ApplicationDbContext _db;
    public CurrentUserCookieEvents(ApplicationDbContext db) => _db = db;

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var idText = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idText, out var id))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
        if (user == null || user.Status != "Active" || user.Role != context.Principal?.FindFirstValue(ClaimTypes.Role))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }
        if (user.FullName != context.Principal?.Identity?.Name || user.Email != context.Principal?.FindFirstValue(ClaimTypes.Email))
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
            context.ReplacePrincipal(new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
            context.ShouldRenew = true;
        }
    }
}
