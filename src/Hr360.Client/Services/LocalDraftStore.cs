using System.Text.Json;
using Hr360.Shared;
using Microsoft.JSInterop;

namespace Hr360.Client.Services;

public sealed class LocalDraftStore(IJSRuntime jsRuntime)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SaveAsync(Guid assignmentId, IReadOnlyList<AnswerDto> answers)
    {
        var json = JsonSerializer.Serialize(answers, JsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", Key(assignmentId), json);
    }

    public async Task<IReadOnlyList<AnswerDto>> LoadAsync(Guid assignmentId)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", Key(assignmentId));
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<IReadOnlyList<AnswerDto>>(json, JsonOptions) ?? [];
    }

    public async Task ClearAsync(Guid assignmentId)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", Key(assignmentId));
    }

    private static string Key(Guid assignmentId) => $"hr360:draft:{assignmentId:N}";
}
