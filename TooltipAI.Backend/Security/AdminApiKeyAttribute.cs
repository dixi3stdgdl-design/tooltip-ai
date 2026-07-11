using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TooltipAI.Backend.Security;

/// <summary>
/// Restricts an endpoint to callers that present a valid admin API key in the
/// <c>X-Admin-Key</c> header. The expected key is read from configuration
/// (<c>Admin:ApiKey</c> or the <c>ADMIN_API_KEY</c> environment variable).
/// Fails closed: if no key is configured the endpoint is unavailable.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class AdminApiKeyAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Admin-Key";

    private static readonly HashSet<string> PlaceholderKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "CHANGE_ME",
        "CHANGE_ME_TO_A_SECURE_KEY_IN_PRODUCTION",
        "changeme",
        "your-admin-key",
    };

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        var expectedKey = configuration["Admin:ApiKey"]
            ?? Environment.GetEnvironmentVariable("ADMIN_API_KEY");

        if (string.IsNullOrWhiteSpace(expectedKey) || PlaceholderKeys.Contains(expectedKey))
        {
            context.Result = new ObjectResult(new { error = "Admin API is not configured" })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        var providedKey = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(providedKey) || !FixedTimeEquals(providedKey, expectedKey))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid or missing admin API key" });
        }
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
