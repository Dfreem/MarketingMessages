using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingMessages.Migrations
{
    /// <inheritdoc />
    public partial class RenameJobType_CampaignType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobType",
                table: "Campaigns",
                newName: "CampaignType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CampaignType",
                table: "Campaigns",
                newName: "JobType");
        }
    }
}
