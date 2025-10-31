using MarketingMessages.Data;
using MarketingMessages.Shared.DTO;

namespace MarketingMessages.Repository
{
    public class CampaignRepository
    {
        MarketingMessagesContext _context;
        ILogger<CampaignRepository> _logger;

        public CampaignRepository(IDbContextFactory<MarketingMessagesContext> contactFactory, ILogger<CampaignRepository> logger)
        {
            _logger = logger;
            _context = contactFactory.CreateDbContext();
        }
        public async Task<CampaignDetailModel> GetCampaignDetailsAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns.Include(c => c.EmailContent).FirstOrDefaultAsync(c => c.CampaignId == id);
                if(campaign is null)
                {
                    _logger.LogError("Unable to find campaign for Campaign Id {id}", id);
                    return new();
                }
                var contacts = _context.ExecuteQueryForList(campaign.AudienceId);
                var stats = await _context.GetCampaignStatsAsync(id);
                CampaignDetailModel model = campaign.ToDetailsDisplay();
                model.Stats = stats;

                model.Contacts = _context.ExecuteQueryForList(campaign.AudienceId).Select(c => new ContactModel(c)).ToList();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex}", ex);
                return new();
            }
        }
    }
}
