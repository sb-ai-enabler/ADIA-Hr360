using System.Net.Http.Json;
using Hr360.Shared;

namespace Hr360.Client.Services;

public sealed class Hr360ApiClient(HttpClient httpClient)
{
    public Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync() =>
        GetAsync<IReadOnlyList<EmployeeDto>>("api/employees");

    public Task<IReadOnlyList<ReviewTemplateDto>> GetTemplatesAsync() =>
        GetAsync<IReadOnlyList<ReviewTemplateDto>>("api/templates");

    public Task<ReviewTemplateDto> CreateTemplateAsync(CreateTemplateRequest request) =>
        PostAsync<CreateTemplateRequest, ReviewTemplateDto>("api/templates", request);

    public Task<IReadOnlyList<ReviewCycleDto>> GetCyclesAsync() =>
        GetAsync<IReadOnlyList<ReviewCycleDto>>("api/cycles");

    public Task<ReviewCycleDto> CreateCycleAsync(CreateCycleRequest request) =>
        PostAsync<CreateCycleRequest, ReviewCycleDto>("api/cycles", request);

    public Task<IReadOnlyList<ReviewAssignmentDto>> GetMyAssignmentsAsync() =>
        GetAsync<IReadOnlyList<ReviewAssignmentDto>>("api/assignments/me");

    public Task<AssignmentFormDto> GetAssignmentFormAsync(Guid assignmentId) =>
        GetAsync<AssignmentFormDto>($"api/assignments/{assignmentId}/form");

    public Task<ReportingSummaryDto> GetReportingSummaryAsync() =>
        GetAsync<ReportingSummaryDto>("api/reports/summary");

    public async Task SaveDraftAsync(SaveDraftRequest request)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/feedback/draft", request);
        await EnsureSuccessAsync(response);
    }

    public async Task SubmitFeedbackAsync(SubmitFeedbackRequest request)
    {
        using var response = await SendAsync(HttpMethod.Post, "api/feedback/submit", request);
        await EnsureSuccessAsync(response);
    }

    private async Task<T> GetAsync<T>(string url)
    {
        using var response = await SendAsync(HttpMethod.Get, url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        using var response = await SendAsync(HttpMethod.Post, url, request);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>() ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await httpClient.SendAsync(request);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        throw new InvalidOperationException(error?.Message ?? $"{(int)response.StatusCode} {response.ReasonPhrase}");
    }
}
