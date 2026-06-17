using Hr360.Api.Auth;
using Hr360.Application.Interfaces;
using Hr360.Shared;

namespace Hr360.Api.Extensions
{
    public static class EndpointRegistrations
    {
        public static void AddApiEndpoints(this WebApplication app)
        {

            var api = app.MapGroup("/api");

            api.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTimeOffset.UtcNow }));

            api.MapGet("/employees", async (IReviewCycleService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetEmployeesAsync(cancellationToken)))
                .RequireAuthorization(Hr360Policies.Admins);

            api.MapGet("/templates", async (ITemplateService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetTemplatesAsync(cancellationToken)))
                .RequireAuthorization(Hr360Policies.Admins);

            api.MapPost("/templates", async (CreateTemplateRequest request, ITemplateService service, CancellationToken cancellationToken) =>
                await ToResult(async () =>
                {
                    var template = await service.CreateTemplateAsync(request, cancellationToken);
                    return Results.Created($"/api/templates/{template.Id}", template);
                }))
                .RequireAuthorization(Hr360Policies.Admins);

            api.MapGet("/cycles", async (IReviewCycleService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetCyclesAsync(cancellationToken)))
                .RequireAuthorization(Hr360Policies.Admins);

            api.MapPost("/cycles", async (CreateCycleRequest request, IReviewCycleService service, CancellationToken cancellationToken) =>
                await ToResult(async () =>
                {
                    var cycle = await service.CreateCycleAsync(request, cancellationToken);
                    return Results.Created($"/api/cycles/{cycle.Id}", cycle);
                }))
                .RequireAuthorization(Hr360Policies.Admins);

            api.MapGet("/assignments/me", async (IFeedbackService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetAssignmentsForCurrentUserAsync(cancellationToken)))
                .RequireAuthorization(Hr360Policies.Employees);

            api.MapGet("/assignments/{assignmentId:guid}/form", async (Guid assignmentId, IFeedbackService service, CancellationToken cancellationToken) =>
                await ToResult(async () => Results.Ok(await service.GetFormAsync(assignmentId, cancellationToken))))
                .RequireAuthorization(Hr360Policies.Employees);

            api.MapPost("/feedback/draft", async (SaveDraftRequest request, IFeedbackService service, CancellationToken cancellationToken) =>
                await ToResult(async () =>
                {
                    await service.SaveDraftAsync(request, cancellationToken);
                    return Results.NoContent();
                }))
                .RequireAuthorization(Hr360Policies.Employees);

            api.MapPost("/feedback/submit", async (SubmitFeedbackRequest request, IFeedbackService service, CancellationToken cancellationToken) =>
                await ToResult(async () =>
                {
                    await service.SubmitAsync(request, cancellationToken);
                    return Results.Accepted();
                }))
                .RequireAuthorization(Hr360Policies.Employees);

            api.MapGet("/reports/summary", async (IReportingService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetSummaryAsync(cancellationToken)))
                .RequireAuthorization(Hr360Policies.Reporting);

        }

        static async Task<IResult> ToResult(Func<Task<IResult>> action)
        {
            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new ApiError("validation_error", ex.Message));
            }
        }
    }
}
