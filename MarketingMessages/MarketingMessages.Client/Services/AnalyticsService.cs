using MarketingMessages.Shared.DTO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MarketingMessages.Client.Services;


public class AnalyticsService
{

    string _baseUrl = "api/Analytics";
    HttpClient _http;
    public AnalyticsService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<StatisticsModel>GetStatsForCampaign(int campaignId)
    {
        string url = $"{_baseUrl}/campaign/{campaignId}";
        var response = await _http.GetAsync(url);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<StatisticsModel>()??new();
        return new();
    }

    public async Task<StatisticsModel> GetStatisticsAsync(DateTime? start, DateTime? end)
    {
        string url = $"{_baseUrl}/general";
        if (start.HasValue)
        {
            url += $"/{start:yyyy-MM-dd}";
            if (end.HasValue)
            {
                url += $"/{end:yyyy-MM-dd}";
            }

        }
        var result = await _http.GetFromJsonAsync<StatisticsModel>(url);
        return result ?? new StatisticsModel();
    }
}