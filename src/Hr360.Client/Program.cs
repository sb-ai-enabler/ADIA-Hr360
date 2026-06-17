using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Hr360.Client;
using Hr360.Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped<LocalDraftStore>();

var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;
var apiScopes = builder.Configuration.GetSection("ServerApi:Scopes").Get<string[]>() ?? [];

builder.Services.AddScoped<ApiAuthorizationMessageHandler>();
builder.Services.AddHttpClient<Hr360ApiClient>(client => client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    foreach (var scope in apiScopes)
    {
        options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
    }

    options.UserOptions.RoleClaim = "roles";
    options.UserOptions.NameClaim = "name";
});

await builder.Build().RunAsync();
