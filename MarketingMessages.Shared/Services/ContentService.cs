using MarketingMessages.Shared.DTO;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

namespace MarketingMessages.Shared.Services;

public class ContentService
{
    private string _baseUrl = "api/Contents";
    private HttpClient _http;
    private ILogger<ContentService> _logger;
    public ContentService(HttpClient http, ILogger<ContentService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<EmailContentResponse>> GetUserContentAsync()
    {
        var response = await _http.GetAsync(_baseUrl);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<EmailContentResponse>>() ?? [];
        }
        _logger.LogError("An error occurred while getting user content\n{reason}", response.ReasonPhrase);
        return [];
    }

    public async Task<EmailContentResponse> GetContentByIdAsync(int contentId)
    {
        string url = $"{_baseUrl}/by-id/{contentId}";
        var response = await _http.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<EmailContentResponse>()??new();
        }
        return new();
    }


    public async Task<SuccessResponse<HtmlEditorContent>> SaveContentAsync(HtmlEditorContent content)
    {
        string url = $"{_baseUrl}/save-content";
        var payload = new EmailRequest(content);
        var response = await _http.PostAsJsonAsync(url, payload);
        if (response.IsSuccessStatusCode)
        {
            content.ContentId = await response.Content.ReadFromJsonAsync<int>();
            return new() { Data = content, Success = true };
        }
        return new() { Error = response.ReasonPhrase };
    }

    public async Task<SuccessResponse> UpdateContentAsync(HtmlEditorContent content)
    {
        string url = $"{_baseUrl}/update-content";
        var payload = new EmailRequest(content);
        var response = await _http.PostAsJsonAsync(url, payload);
        return new() { Error = response.ReasonPhrase, Success = response.IsSuccessStatusCode };
    }

    public async Task<ImageUploadResponse> UploadImageAsync(string name, string base64)
    {
        string url = $"{_baseUrl}/upload-image/{name}";
        var response = await _http.PostAsJsonAsync(url, base64);
        var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>()??new();
        return result;
    }

    public async Task<ImageUploadResponse> UploadImageAsync(HttpRequestMessage request)
    {
        string url = $"api/image/upload-image";
        request.RequestUri = new Uri(url);
        request.Method = HttpMethod.Post;
        var response = await _http.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ImageUploadResponse>()??new();
        return new();

    }

    public async Task<SuccessResponse> DeleteEmailContentAsync(int contentId)
    {
        string url = $"{_baseUrl}/{contentId}";
        var response = await _http.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

}
