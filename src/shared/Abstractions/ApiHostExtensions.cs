using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HealthTech.BuildingBlocks.Abstractions;

public static class ApiHostExtensions
{
    public static IServiceCollection AddHealthTechJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var authSection = configuration.GetSection("Auth");

                options.Authority = authSection["Authority"];
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authSection["Issuer"],
                    ValidateAudience = !string.IsNullOrWhiteSpace(authSection["Audience"]),
                    ValidAudience = authSection["Audience"],
                    ValidateLifetime = true
                };
            })
            .AddScheme<InternalGatewayAuthenticationOptions, InternalGatewayAuthenticationHandler>(
                InternalGatewayAuthenticationDefaults.Scheme,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    InternalGatewayAuthenticationDefaults.Scheme)
                .RequireAuthenticatedUser()
                .Build();
        });
        return services;
    }
}
