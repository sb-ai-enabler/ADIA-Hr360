using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace Hr360.Client.Services;

/// <summary>
/// Attaches an Entra access token to outbound requests targeting the HR360 API.
/// The API runs on a separate origin, so the authorized URL and scopes are
/// configured explicitly rather than relying on the app base address.
/// </summary>
public sealed class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        NavigationManager navigation,
        IConfiguration configuration)
        : base(provider, navigation)
    {
        var apiBaseAddress = configuration["ApiBaseAddress"] ?? navigation.BaseUri;
        var scopes = configuration.GetSection("ServerApi:Scopes").Get<string[]>() ?? [];

        ConfigureHandler(
            authorizedUrls: [apiBaseAddress],
            scopes: scopes);
    }
}
