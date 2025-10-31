using MarketingMessages.Shared.DTO;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Client.Services;

public class CampaignService
{
    private string _baseUrl = "api/Campaigns";
    private HttpClient _http;
    private ILogger<CampaignService> _logger;
    public CampaignService(HttpClient http, ILogger<CampaignService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SuccessResponse> TestCampaignEmail(int campaignId, string email)
    {
        string url = $"{_baseUrl}/test-campaign/{campaignId}/{email}";
        var response =await _http.GetAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    public async Task<List<CampaignDetailModel>> GetUserCampaignsAsync()
    {
        return await _http.GetFromJsonAsync<List<CampaignDetailModel>>(_baseUrl) ?? [];

    }

    public async Task<SuccessResponse<CampaignDetailModel>> SaveCampaignAsync(WizardRequest request)
    {
        var response = await _http.PostAsJsonAsync(_baseUrl, request);
        CampaignDetailModel model = await response.Content.ReadFromJsonAsync<CampaignDetailModel>() ?? new();
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase, Data = model };
    }
}
