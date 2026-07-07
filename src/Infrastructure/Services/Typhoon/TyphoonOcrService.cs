using Application.Commons.Exceptions;
using Application.Commons.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using System.Net.Http.Headers;

namespace Infrastructure.Services.Typhoon;

public sealed class TyphoonOcrService : ITyphoonOcrService
{
    private readonly HttpClient _httpClient;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly string _model;

    public TyphoonOcrService(
        HttpClient httpClient,
        RecyclableMemoryStreamManager streamManager,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _streamManager = streamManager;
        _model = configuration["Typhoon:OcrModel"] ?? "typhoon-ocr";

        var apiKey = configuration["Typhoon:ApiKey"]
            ?? throw new InvalidOperationException("Typhoon:ApiKey is not configured.");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var endpoint = configuration["Typhoon:OcrEndpoint"]
            ?? "https://api.opentyphoon.ai/v1/ocr";
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(endpoint);
        }
    }

    public async Task<string> OcrAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        // Use RecyclableMemoryStream for the file content to avoid a second heap copy
        using var ms = _streamManager.GetStream("typhoon-ocr", imageBytes, 0, imageBytes.Length);
        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(_model), "model");

        var response = await _httpClient.PostAsync(string.Empty, form, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new TyphoonOcrException((int)response.StatusCode, $"OCR request failed: {response.StatusCode}", body);
        }

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        // Typhoon OCR v1.5 returns JSON with a "markdown" field (or raw markdown).
        // Parse defensively: try JSON first, fall back to raw string.
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(result);
            if (doc.RootElement.TryGetProperty("markdown", out var md) && md.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return md.GetString() ?? result;
            }
            if (doc.RootElement.TryGetProperty("results", out var results) && results.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                // Concatenate all result text blocks
                return string.Join("\n", results.EnumerateArray()
                    .Select(r => r.TryGetProperty("text", out var t) ? t.GetString() : null));
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Not JSON — treat as raw markdown
        }

        return result;
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            _ => "image/jpeg"
        };
    }
}
