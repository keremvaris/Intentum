using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Intentum.AspNetCore.Auth;

/// <summary>
/// Extension methods for configuring Intentum authentication (JWT + API key).
/// </summary>
public static class IntentumAuthExtensions
{
    private const string RoleAdmin = "Admin";

    /// <summary>
    /// Adds Intentum authentication with API key support.
    /// </summary>
    public static AuthenticationBuilder AddIntentumApiKeyAuth(
        this IServiceCollection services,
        Action<ApiKeyAuthenticationOptions> configure)
    {
        return services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, configure);
    }

    /// <summary>
    /// Adds Intentum authentication with both JWT and API key support.
    /// </summary>
    public static AuthenticationBuilder AddIntentumAuth(
        this IServiceCollection services,
        Action<IntentumAuthOptions> configure)
    {
        var options = new IntentumAuthOptions();
        configure(options);

        var builder = services.AddAuthentication(opts =>
        {
            opts.DefaultScheme = "IntentumComposite";
            opts.DefaultChallengeScheme = "IntentumComposite";
        });

        if (!string.IsNullOrEmpty(options.JwtSecret))
        {
            builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(options.JwtIssuer),
                    ValidIssuer = options.JwtIssuer,
                    ValidateAudience = !string.IsNullOrEmpty(options.JwtAudience),
                    ValidAudience = options.JwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSecret)),
                    ValidateLifetime = true
                };
            });
        }

        builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationHandler.SchemeName, apiKeyOpts =>
            {
                apiKeyOpts.ValidKeys = options.ApiKeys;
            });

        builder.AddPolicyScheme("IntentumComposite", "JWT or API Key", policyOpts =>
        {
            policyOpts.ForwardDefaultSelector = context =>
            {
                if (context.Request.Headers.ContainsKey("Authorization"))
                    return JwtBearerDefaults.AuthenticationScheme;
                if (context.Request.Headers.ContainsKey("X-Api-Key") ||
                    context.Request.Query.ContainsKey("api_key"))
                    return ApiKeyAuthenticationHandler.SchemeName;
                return JwtBearerDefaults.AuthenticationScheme;
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(RoleAdmin, policy => policy.RequireRole(RoleAdmin))
            .AddPolicy("Analyst", policy => policy.RequireRole(RoleAdmin, "Analyst"))
            .AddPolicy("ReadOnly", policy => policy.RequireRole(RoleAdmin, "Analyst", "ReadOnly"));

        return builder;
    }
}

/// <summary>
/// Combined authentication options for JWT + API key.
/// </summary>
public sealed class IntentumAuthOptions
{
    public string? JwtSecret { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtAudience { get; set; }
    public List<ApiKeyEntry> ApiKeys { get; set; } = [];
}
