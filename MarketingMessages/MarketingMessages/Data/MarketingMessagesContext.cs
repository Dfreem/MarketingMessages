using MarketingMessages.Shared.Models;

using Sender = MarketingMessages.Shared.Models.Sender;

namespace MarketingMessages.Data;

public partial class MarketingMessagesContext : DbContext
{
    public MarketingMessagesContext(DbContextOptions<MarketingMessagesContext> options) : base(options)
    {
    }

    public virtual DbSet<ApplicationSetting> Settings { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<SuppressionGroup> SuppressionGroups { get; set; }
    public virtual DbSet<JobLog> JobLogs { get; set; }
    public virtual DbSet<Audience> Audiences { get; set; }

    public virtual DbSet<Sender> Senders { get; set; }

    public virtual DbSet<SentLog> SentLogs { get; set; }
    public virtual DbSet<ContentImage> Images { get; set; }
    public virtual DbSet<EmailContentImages> ContentImages { get; set; }

    public virtual DbSet<EngagementUrl> EngagementUrls { get; set; }

    //public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Shared.Models.EmailEvent> EmailEvents { get; set; }
    public virtual DbSet<ApplicationLog> ApplicationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}