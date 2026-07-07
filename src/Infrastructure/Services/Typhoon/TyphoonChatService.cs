using Application.Commons.Services;
using Application.Features.IdCardExtractions.Queries.ExtractIdCard;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace Infrastructure.Services.Typhoon;

public sealed class TyphoonChatService : ITyphoonChatService
{
    private readonly ChatClient _chatClient;

    private const string SystemPrompt = """
        You are an ID-card data extractor. From the provided OCR markdown of a Thai national ID card,
        return a JSON object with exactly these keys: "nationalId", "firstName", "lastName",
        "birthDate" (ISO 8601 yyyy-MM-dd), "addressLine1", "provinceName", "districtName",
        "subDistrictName", "postalCode". For any field you cannot confidently extract, return null
        for that key. Do not include any text outside the JSON object. Do not wrap the JSON in
        markdown fences.
        """;

    public TyphoonChatService(IConfiguration configuration)
    {
        var apiKey = configuration["Typhoon:ApiKey"]
            ?? throw new InvalidOperationException("Typhoon:ApiKey is not configured.");
        var endpoint = configuration["Typhoon:ChatEndpoint"]
            ?? "https://api.opentyphoon.ai/v1";
        var model = configuration["Typhoon:ChatModel"]
            ?? "typhoon-v2.5-30b-a3b-instruct";

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        _chatClient = client.GetChatClient(model);
    }

    public async Task<IdCardData> ExtractIdCardAsync(string markdown, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(markdown)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.1f,
            MaxOutputTokenCount = 512,
            TopP = 0.6f
        };

        // API/network errors bubble up to ExtractIdCard.RequestHandler which returns Result.Failure.
        // Only JSON parse failures (handled in ParseIdCardJson) are treated as graceful degradation.
        var completion = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        var content = completion.Value.Content[0].Text;
        return ParseIdCardJson(content);
    }

    private static IdCardData ParseIdCardJson(string content)
    {
        // Strip markdown fences if present (defensive — the prompt forbids them)
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0) trimmed = trimmed[(firstNewline + 1)..];
            var lastFence = trimmed.LastIndexOf("```");
            if (lastFence >= 0) trimmed = trimmed[..lastFence];
        }

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;
            return new IdCardData
            {
                NationalId = GetStringOrNull(root, "nationalId"),
                FirstName = GetStringOrNull(root, "firstName"),
                LastName = GetStringOrNull(root, "lastName"),
                BirthDate = GetDateTimeOrNull(root, "birthDate"),
                AddressLine1 = GetStringOrNull(root, "addressLine1"),
                ProvinceName = GetStringOrNull(root, "provinceName"),
                DistrictName = GetStringOrNull(root, "districtName"),
                SubDistrictName = GetStringOrNull(root, "subDistrictName"),
                PostalCode = GetStringOrNull(root, "postalCode")
            };
        }
        catch (JsonException)
        {
            return new IdCardData();
        }
    }

    private static string? GetStringOrNull(JsonElement root, string name)
        => root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;

    private static DateTime? GetDateTimeOrNull(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var el) || el.ValueKind != JsonValueKind.String)
            return null;
        return DateTime.TryParse(el.GetString(), out var d) ? d : null;
    }
}