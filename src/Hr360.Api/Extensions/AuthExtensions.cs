using Hr360.Api.Auth;
using Hr360.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Hr360.Api.Extensions
{
    public static class AuthExtensions
    {
        extension(IServiceCollection services)
        {
            public void AddHr360Authentication(IConfiguration configuration)
            {
                var authSection = configuration.GetSection("Authentication");
                var authority = authSection["Authority"];
                var audience = authSection["Audience"];

                if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(audience))
                {
                    throw new InvalidOperationException(
                        "Authentication:Authority and Authentication:Audience must be configured.");
                }

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authority;
                        options.Audience = audience;
                        options.RequireHttpsMetadata = true;
                        options.MapInboundClaims = false;
                        options.TokenValidationParameters.RoleClaimType = "roles";
                        options.TokenValidationParameters.ValidateIssuer = true;
                        options.TokenValidationParameters.ValidateAudience = true;
                        options.TokenValidationParameters.ValidateLifetime = true;
                        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    });
            }

            public void AddHr360Authorization()
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Hr360Policies.Employees, policy => policy.RequireAuthenticatedUser());
                    options.AddPolicy(Hr360Policies.Admins, policy => policy.RequireRole(Hr360Roles.HrAdmin, Hr360Roles.SystemAdmin));
                    options.AddPolicy(Hr360Policies.Reporting, policy => policy.RequireRole(Hr360Roles.HrAdmin, Hr360Roles.Manager, Hr360Roles.SystemAdmin));
                });
            }
        }
    }
}
