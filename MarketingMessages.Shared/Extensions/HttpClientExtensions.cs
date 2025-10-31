using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using MarketingMessages.Shared.Components.HtmlEditor;
using MarketingMessages.Shared.Models;

namespace MarketingMessages.Shared.Extensions;

public static class HttpClientExtensions
{
    #region Utility

    public static async Task<SuccessResponse<ListFormModel>> GetListFormOptions(this HttpClient client)
    {
        string url = "api/SendList/form-options";
        var response = await client.GetAsync(url);
        ListFormModel result = new();
        if (response.IsSuccessStatusCode)
        {
            result = await response.Content.ReadFromJsonAsync<ListFormModel>() ?? new();
        }
        return new() { Data = result, Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    #endregion

    #region Send Lists
    public static async Task<SuccessResponse<Dictionary<string, int>>> GetUserSendListsAsync(this HttpClient client)
    {
        string url = "api/SendList/user-lists";
        var response = await client.GetAsync(url);
        Dictionary<string, int> result = [];
        if (response.IsSuccessStatusCode)
        {
            result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>() ?? [];
        }
        return new() { Data = result, Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    public static async Task<SuccessResponse<int>> SaveSendListAsync(this HttpClient client, SendListRequest request)
    {

        string url = "api/SendList/user-lists";
        var response = await client.PutAsJsonAsync(url, request);
        int result = 0;
        if (response.IsSuccessStatusCode)
        {
            result = Convert.ToInt32(await response.Content.ReadAsStringAsync());
        }
        return new() { Data = result, Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }


    public static async Task<SuccessResponse<int>> QuerySendListAsync(this HttpClient client, SendListRequest request)
    {

        string url = "api/SendList/query-contacts";
        try
        {

            var response = await client.PostAsJsonAsync(url, request);
            int result = 0;
            if (response.IsSuccessStatusCode)
            {
                result = Convert.ToInt32(await response.Content.ReadAsStringAsync());
            }
            return new() { Data = result, Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}", ex.Message);
            return new() { Data = 0, Success = false, Error = ex.Message };
        }
    }

    public static async Task<SuccessResponse<int>> GetCountForListAsync(this HttpClient client, int listId)
    {

        string url = $"api/SendList/count-by-id/{listId}";
        try
        {

            var response = await client.GetFromJsonAsync<int>(url);
            return new() { Data = response, Success = true };
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}", ex.Message);
            return new() { Data = 0, Success = false, Error = ex.Message };
        }
    }
    public static async Task<SuccessResponse<SendListResponse>> GetProfessionsForListAsync(this HttpClient client, int listId)
    {

        string url = $"api/SendList/professions-by-id/{listId}";
        try
        {
            var response = await client.GetFromJsonAsync<SendListResponse>(url);

            return new() { Data = response, Success = true };
        }
        catch (Exception ex)
        {
            Console.WriteLine("{0}", ex.Message);
            return new() { Data = new(), Success = false, Error = ex.Message };
        }
    }

    public static async Task<SuccessResponse<int>> SearchCountAsync(this HttpClient client, string search)
    {

        // preliminary cleaning before sending to server.
        // proper sanitization is done on the server
        search = search.Replace(";", "").Replace("'", "").Replace(",", "");
        string url = $"api/SendList/search/{search}";
        var response = await client.GetAsync(url);
        int result = 0;
        if (response.IsSuccessStatusCode)
        {
            result = Convert.ToInt32(await response.Content.ReadAsStringAsync());
        }
        return new() { Data = result, Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    #endregion

    #region Contact Lists
    public static async Task<SuccessResponse<AudiencePageResponse>> GetUserContactsAsync(this HttpClient client, ContactsRequest request)
    {
        try
        {

            var listsResponse = await client.PostAsJsonAsync($"api/SendList/contacts", request);
            if (listsResponse.IsSuccessStatusCode)
            {
                var response = await listsResponse.Content.ReadFromJsonAsync<AudiencePageResponse>();
                if (response is not null)
                    return new() { Data = response, Success = true };
            }
            return new() { Success = false, Error = "No contacts where found." };
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while attempting to get user's contacts\n{0}", ex.AsString());
            return new() { Error = ex.Message };
        }


    }

    public static async Task<SuccessResponse> SaveContactListAsync(this HttpClient client, ContactList list)
    {

        string url = "api/ContactList/list";
        var saveResponse = await client.PostAsJsonAsync(url, list);
        return new() { Success = saveResponse.IsSuccessStatusCode, Error = saveResponse.ReasonPhrase };
    }

    public static async Task<SuccessResponse> ScheduleEmailAsync(this HttpClient client, EmailSchedule schedule)
    {
        string url = "api/Email/schedule-job";
        var scheduleResponse = await client.PostAsJsonAsync(url, schedule);
        return new()
        {
            Success = scheduleResponse.IsSuccessStatusCode,
            Error = scheduleResponse.ReasonPhrase
        };
    }

    public static async Task<SuccessResponse<List<EmailSender>>> GetEmailSendersAsync(this HttpClient client)
    {
        string url = "api/SendList/senders";
        var sendersResponse = await client.GetAsync(url);
        if (sendersResponse.IsSuccessStatusCode)
        {
            var senders = await sendersResponse.Content.ReadFromJsonAsync<List<EmailSender>>() ?? [];
            return new SuccessResponse<List<EmailSender>>()
            {
                Success = true,
                Data = senders,
            };
        }
        return new()
        {
            Success = false,
            Error = sendersResponse.ReasonPhrase ?? await sendersResponse.Content.ReadAsStringAsync()
        };
    }

    public static async Task<SuccessResponse<EmailSender>> SaveEmailSenderAsync(this HttpClient client, EmailSender sender)
    {
        string url = "api/ContactList/senders";
        var senderResponse = await client.PostAsJsonAsync(url, sender);
        sender = await senderResponse.Content.ReadFromJsonAsync<EmailSender>() ?? sender;
        return new() { Success = senderResponse.IsSuccessStatusCode, Error = senderResponse.ReasonPhrase, Data = sender };
    }

    public static async Task<SuccessResponse<List<EmailSchedule>>> GetScheduledEmailsAsync(this HttpClient client)
    {
        string url = "api/Email/jobs";
        var response = await client.GetAsync(url);
        return new() { Data = await response.Content.ReadFromJsonAsync<List<EmailSchedule>>(), Error = response.ReasonPhrase, Success = response.IsSuccessStatusCode };

    }

    public static async Task<SuccessResponse> DeleteContactsAsync(this HttpClient client, List<int> contacts)
    {

        string url = $"api/SendList/remove-contacts";
        var response = await client.PostAsJsonAsync(url, contacts);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };

    }

    public static async Task<SuccessResponse> DeleteListAsync(this HttpClient client, int listId)
    {
        string url = $"api/SendList/delete/{listId}";
        var response = await client.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };

    }
    #endregion

    #region Email Jobs

    public static async Task<SuccessResponse> DeleteEmailJobAsync(this HttpClient client, int jobId)
    {
        string url = $"api/Email/delete-job/{jobId}";
        var response = await client.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    #endregion

    #region EmailContent

    public static async Task<SuccessResponse<List<EmailContentResponse>>> GetSavedEmailContentAsync(this HttpClient client)
    {
        string url = "api/Email/get-content";
        var response = await client.GetAsync(url);
        List<EmailContentResponse> result = [];
        if (response.IsSuccessStatusCode)
        {
            result = await response.Content.ReadFromJsonAsync<List<EmailContentResponse>>() ?? [];
        }
        return new() { Data = result, Error = response.ReasonPhrase, Success = response.IsSuccessStatusCode };

    }

    public static async Task<SuccessResponse> DeleteEmailContentAsync(this HttpClient client, int contentId)
    {
        string url = $"api/Email/delete-content/{contentId}";
        var response = await client.DeleteAsync(url);
        return new() { Success = response.IsSuccessStatusCode, Error = response.ReasonPhrase };
    }

    #endregion

    #region Admin

    public static async Task<List<ApplicationLog>> GetApplicationLogsAsync(this HttpClient client, ApplicationLogRequest request)
    {
        string url = $"api/AdminLogs/application-logs";
        var response = await client.PostAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<ApplicationLog>>() ?? [];
        }
        return [];
    }
    public static async Task<List<SentLog>> GetSentLogsAsync(this HttpClient client, DateTime? fromDate, DateTime? toDate)
    {
        string url = $"api/AdminLogs/sent-logs?fromDate={fromDate}&toDate={toDate}";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<SentLog>>() ?? [];
        }
        return [];
    }

    public static async Task<List<ApplicationSetting>> GetAdminSettingsAsync(this HttpClient client)
    {
        string url = "api/ApplicationSettings";

        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<ApplicationSetting>>() ?? [];
        }
        return [];
    }


    public static async Task<SuccessResponse<List<ApplicationSetting>>> UpdateAdminSettingsAsync(this HttpClient client, List<ApplicationSetting> settings)
    {
        string url = "api/ApplicationSettings/update";

        var response = await client.PostAsJsonAsync(url, settings);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<ApplicationSetting>>() ?? [];
            return new() { Data = result, Success = true };
        }
        return new() { Error = response.ReasonPhrase };
    }

    #endregion
}
