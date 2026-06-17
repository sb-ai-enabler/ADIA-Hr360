using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Hr360.Domain;
using Hr360.Infrastructure;
using Hr360.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hr360.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Returns_Ok()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_Read_Seeded_Employees_And_Templates()
    {
        var employees = await _client.GetFromJsonAsync<IReadOnlyList<EmployeeDto>>("/api/employees");
        var templates = await _client.GetFromJsonAsync<IReadOnlyList<ReviewTemplateDto>>("/api/templates");

        Assert.NotNull(employees);
        Assert.NotNull(templates);
        Assert.True(employees.Count >= 4);
        Assert.NotEmpty(templates);
    }
}

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Real JWT validation is not exercised here; supply config so startup passes,
        // then replace the authentication scheme with a deterministic test handler.
        builder.UseSetting("Authentication:Authority", "https://login.test/");
        builder.UseSetting("Authentication:Audience", "api://hr360-api");
        builder.UseSetting("ConnectionStrings:Hr360", "");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Hr360DbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            SeedDatabase(db);
        });
    }

    private static void SeedDatabase(Hr360DbContext db)
    {
        db.Employees.AddRange(
            new Employee { EntraObjectId = "demo.admin", DisplayName = "Demo Admin", Email = "admin@example.com" },
            new Employee { EntraObjectId = "demo.alex", DisplayName = "Alex", Email = "alex@example.com" },
            new Employee { EntraObjectId = "demo.sam", DisplayName = "Sam", Email = "sam@example.com" },
            new Employee { EntraObjectId = "demo.taylor", DisplayName = "Taylor", Email = "taylor@example.com" });

        db.Templates.Add(new ReviewTemplate
        {
            Name = "Core 360",
            Description = "Seeded review template for integration tests.",
            DefinitionJson = """
                {"sections":[{"id":"impact","title":"Impact","questions":[{"id":"impact-rating","prompt":"Rate impact.","type":1,"required":true,"minRating":1,"maxRating":5,"helpText":null}]}]}
                """,
            CreatedBy = "demo.admin"
        });

        db.SaveChanges();
    }
}

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string userId = "demo.admin";
        var claims = new List<Claim>
        {
            new("oid", userId),
            new(ClaimTypes.NameIdentifier, userId),
            new("name", "Test Admin"),
            new(ClaimTypes.Role, Hr360Roles.HrAdmin),
            new("roles", Hr360Roles.HrAdmin)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
