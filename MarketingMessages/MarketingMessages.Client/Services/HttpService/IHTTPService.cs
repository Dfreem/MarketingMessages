using Microsoft.AspNetCore.Components.Forms;
using MarketingMessages.Shared.DTO;
using MarketingMessages.Shared.DTO.V1;
using MarketingMessages.Shared.Models;

namespace MarketingMessages.Client.Services.HttpService
{
    public interface IHTTPService
    {
        Task<SenderModel> GetUserSenderAsync();
        Task<List<AudienceFormModel>> GetAudienceFormRowsAsync();
        Task<SuccessResponse<CsvContactsUpload>> UploadContactFileAsync(IBrowserFile file);
        Task<SuccessResponse<SaveContactsUploadResponse>> SaveContactUploadAsync(CsvContactsUpload csv);
        Task<int> CountAudienceContactsAsync(AudienceFormModel audience);
        Task<SuccessResponse> SaveAudienceAsync(AudienceFormModel audience);
        Task<SuccessResponse> SendOrScheduleEmailAsync(WizardRequest request);
        Task<SuccessResponse> SendTestEmail();
        Task<List<EmailContentResponse>> GetSavedUserContent();
        Task<SuccessResponse> DeleteEmailContentAsync(int contentId);
        Task<SuccessResponse> TestWebhookAsync(EmailEvent[] webhookEvents);
        Task<Dictionary<string, int>> GetUserCampaignsAsync();
        Task<CampaignDetailModel> GetCampaignByIdAsync(int campaignId);
        Task<List<string>> GetProfessions();
        Task<AudiencePageResponse?> GetContactsPageAsync(ContactsRequest request);
        Task<SuccessResponse> DeleteContactsAsync(List<int> contacts);
        Task<List<AudienceSegment>> GetUserAudiencesAsync();
        Task<SuccessResponse> DeleteCampaignAsync(int campaignId);
        Task<SuccessResponse> DeleteAudienceAsync(int audienceId);
        Task<StatisticsModel> GetStatisticsAsync(DateTime? start, DateTime? end);
        Task<SuccessResponse<Audience>> UpdateAudienceAsync(Audience audience);
        Task<SuccessResponse> UpdateCampaignAsync(CampaignDetailModel campaign);
        Task<SuccessResponse> SendTestEmail(HtmlEditorContent content);
    }
}