using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intentum.AspNetCore.Auth;

/// <summary>
/// Authentication handler that validates API keys from the X-Api-Key header or query parameter.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string SchemeName = "ApiKey";
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string ApiKeyQueryName = "api_key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TryGetApiKey(out var apiKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        var validKey = Options.ValidKeys.FirstOrDefault(k => k.Key == apiKey);
        if (validKey == null)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, validKey.Name),
            new(ClaimTypes.NameIdentifier, validKey.Key),
        };
        foreach (var role in validKey.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryGetApiKey(out string apiKey)
    {
        apiKey = "";

        if (Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            apiKey = headerValue.ToString();
            return true;
        }

        if (Request.Query.TryGetValue(ApiKeyQueryName, out var queryValue) &&
            !string.IsNullOrWhiteSpace(queryValue))
        {
            apiKey = queryValue.ToString();
            return true;
        }

        return false;
    }
}

/// <summary>
/// Options for API key authentication.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public List<ApiKeyEntry> ValidKeys { get; set; } = [];
}

/// <summary>
/// Represents a registered API key with associated name and roles.
/// </summary>
public sealed class ApiKeyEntry
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public List<string> Roles { get; init; } = [];
}
