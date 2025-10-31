using MarketingMessages.Shared.DTO;

using Microsoft.Extensions.Logging;

using System.Net.Http.Json;

using MarketingMessages.Shared.Components.HtmlEditor;

namespace MarketingMessages.Client.Services;

public class AudienceService
{
    private string _baseUrl = "api/Audiences";
    private HttpClient _http;
    private ILogger<AudienceService> _logger;
    public AudienceService(HttpClient http, ILogger<AudienceService> logger)
    {
        _http = http;
        _logger = logger;
    }
    public async Task<AudienceFormModel?> GetSegmentForEditing(int segmentId)
    {
        return await _http.GetFromJsonAsync<AudienceFormModel>($"{_baseUrl}/segment-form/{segmentId}");
    }

    public async Task<int> GetUserTotalContacts()
    {
        return await _http.GetFromJsonAsync<int>($"{_baseUrl}/total-contacts");
    }

    public async Task<SuccessResponse> DeletAllContactsAsync()
    {
        var response = await _http.GetAsync($"{_baseUrl}/remove-contacts");
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }
}
