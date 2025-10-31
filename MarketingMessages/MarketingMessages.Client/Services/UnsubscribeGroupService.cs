using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using MarketingMessages.Shared.DTO;

namespace MarketingMessages.Client.Services;

public class UnsubscribeGroupService
{
    private string _baseUrl = "api/UnsubscribeGroups";
    private HttpClient _http;
    private ILogger<UnsubscribeGroupService> _logger;

    public UnsubscribeGroupService(HttpClient http, ILogger<UnsubscribeGroupService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<SuppressionGroupModel>> GetUserGroupsAsync()
    {
        return await _http.GetFromJsonAsync<List<SuppressionGroupModel>>(_baseUrl)??[];
    }
}
