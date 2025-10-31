using AngleSharp.Css;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Logging;

using System.Net.Http.Json;
using System.Threading.Tasks;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.Extensions;
using MarketingMessages.Shared.Models;

namespace MarketingMessages.Client.Services.HttpService;

public class HTTPService : IHTTPService
{
    private HttpClient _http;
    private ILogger<HTTPService> _logger;

    public HTTPService(HttpClient client, ILogger<HTTPService> logger)
    {
        _http = client;
        _logger = logger;
    }

    public async Task<List<AudienceFormModel>> GetAudienceFormRowsAsync()
    {
        return await _http.GetFromJsonAsync<List<AudienceFormModel>>("/api/Audiences/segments") ?? [];
    }

    public async Task<List<AudienceSegment>> GetUserAudiencesAsync()
    {
        string url = "api/audiences";
        return await _http.GetFromJsonAsync<List<AudienceSegment>>(url) ?? [];
    }

    public async Task<Dictionary<string, int>> GetUserCampaignsAsync()
    {
        string url = "api/Campaigns/";
        return await _http.GetFromJsonAsync<Dictionary<string, int>>(url) ?? [];

    }

    public async Task<SenderModel> GetUserSenderAsync()
    {
        return await _http.GetFromJsonAsync<SenderModel>("/api/Senders") ?? new();
    }

    public async Task<SuccessResponse<CsvContactsUpload>> UploadContactFileAsync(IBrowserFile file)
    {
        using var content = new StreamContent(file.OpenReadStream(1024000));
        // _loadingFile = false;
        // await InvokeAsync(StateHasChanged);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        MultipartFormDataContent formDataContent = new();
        formDataContent.Add(content, name: "csv", fileName: file.Name);
        using HttpRequestMessage requestMessage = new(HttpMethod.Post, "api/Audiences/upload");
        requestMessage.SetBrowserRequestStreamingEnabled(true);
        requestMessage.Content = formDataContent;
        try
        {

            var response = await _http.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                CsvContactsUpload csv = await response.Content.ReadFromJsonAsync<CsvContactsUpload>() ?? new();
                return new() { Success = true, Data = csv };
            }
            return new() { Error = response.ReasonPhrase };
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
            return new() { Error = ex.Message };
        }


    }

    public async Task<AudiencePageResponse?> GetContactsPageAsync(ContactsRequest request)
    {
        string url = $"api/Audiences/page-contacts";
        var response = await _http.PostAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AudiencePageResponse>();
        }
        return null;
    }

    public async Task<List<string>> GetProfessions()
    {

        string url = $"/api/Audiences/professions";
        return await _http.GetFromJsonAsync<List<string>>(url) ?? [];
    }

    public async Task<int> CountAudienceContactsAsync(AudienceFormModel audience)
    {
        string url = "api/Audiences/count";
        var response = await _http.PostAsJsonAsync(url, audience);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<int>();
        return 0;
    }

    public async Task<SuccessResponse> SendTestEmail()
    {
        var response = await _http.GetAsync("api/Email/test-email");
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase, Message = await response.Content.ReadAsStringAsync() };

    }

    public async Task<SuccessResponse> SendTestEmail(HtmlEditorContent content)
    {
        var response = await _http.PostAsJsonAsync("api/Email/test-email", content);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase, Message = await response.Content.ReadAsStringAsync() };
    }

    public async Task<SuccessResponse> SendOrScheduleEmailAsync(WizardRequest request)
    {
        string url = "api/Email/schedule-job";
        var scheduleResponse = await _http.PostAsJsonAsync(url, request);
        return new()
        {
            Success = scheduleResponse.IsSuccessStatusCode,
            Error = scheduleResponse.ReasonPhrase
        };
    }

    public async Task<List<EmailContentResponse>> GetSavedUserContent()
    {
        string url = "api/Email/get-content";
        var response = await _http.GetAsync(url);
        if(response.IsSuccessStatusCode)
        {
            var debug = await  response.Content.ReadAsStringAsync();
            Console.WriteLine(debug);
        }
        return await response.Content.ReadFromJsonAsync<List<EmailContentResponse>>()??[];

    }
    public async Task<SuccessResponse> DeleteEmailContentAsync(int contentId)
    {
        string url = $"api/Email/delete-content/{contentId}";
        var response = await _http.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }
    public async Task<SuccessResponse> DeleteCampaignAsync(int campaignId)
    {
        string url = $"api/Campaigns/{campaignId}";
        var response = await _http.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    public async Task<SuccessResponse> DeleteAudienceAsync(int audienceId)
    {
        string url = $"api/Audiences/delete-audience/{audienceId}";
        var response = await _http.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    public async Task<SuccessResponse> DeleteContactsAsync(List<int> contacts)
    {
        string url = $"api/audience/remove-contacts";
        var response = await _http.PostAsJsonAsync(url, contacts);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    public async Task<CampaignDetailModel> GetCampaignByIdAsync(int campaignId)
    {
        string url = $"api/Campaigns/{campaignId}";
        var result = await _http.GetFromJsonAsync<CampaignDetailModel>(url);
        if (result is null)
            throw new Exception($"An error occurred while attempting to get campaign with Id {campaignId}");
        return result;
    }

    public async Task<StatisticsModel> GetStatisticsAsync(DateTime? start, DateTime? end)
    {
        string url = "api/Analytics/general";
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
    public async Task<SuccessResponse<SaveContactsUploadResponse>> SaveContactUploadAsync(CsvContactsUpload csv)
    {

        string url = "api/Audiences/save-upload";
        var uploadResponse = await _http.PostAsJsonAsync(url, csv);
        if (uploadResponse.IsSuccessStatusCode)
        {
            var results = await uploadResponse.Content.ReadFromJsonAsync<SaveContactsUploadResponse>();
            return new() { Data = results, Success = true };
        }
        return new() { Error = uploadResponse.ReasonPhrase };
    }
    public async Task<SuccessResponse> SaveAudienceAsync(AudienceFormModel audience)
    {
        string url = "api/Audiences/save";
        var response = await _http.PostAsJsonAsync(url, audience);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    public async Task<SuccessResponse<Audience>> UpdateAudienceAsync(Audience audience)
    {
        string url = "api/Audiences/update";
        try
        {
            var response = await _http.PutAsJsonAsync(url, audience);
            if (response.IsSuccessStatusCode)
            {
                audience = await response.Content.ReadFromJsonAsync<Audience>() ?? audience;
            }
            return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase, Data = audience };
        }
        catch (Exception ex)
        {
            _logger.LogError("{ex}", ex);
            return new() { Error = ex.Message };
        }
    }

    public async Task<SuccessResponse> UpdateCampaignAsync(CampaignDetailModel campaign)
    {
        if (campaign.CampaignId == 0)
            return new(){ };
        var response = await _http.PutAsJsonAsync($"api/Campaigns/{campaign.CampaignId}", campaign);
        return new() { Success = response.IsSuccessStatusCode };
    }

    public async Task<SuccessResponse> TestWebhookAsync(EmailEvent[] webhookEvents)
    {
        string url = "api/Engagement/webhook";
        var response = await _http.PostAsJsonAsync(url, webhookEvents);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }
}
