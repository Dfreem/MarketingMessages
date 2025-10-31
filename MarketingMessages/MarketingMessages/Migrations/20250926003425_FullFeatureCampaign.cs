using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingMessages.Migrations
{
    /// <inheritdoc />
    public partial class FullFeatureCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Campaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FrequencyInterval",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FrequencyUnit",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Campaigns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "Campaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobsCompleted",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextExecution",
                table: "Campaigns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalJobLimit",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "FrequencyInterval",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "FrequencyUnit",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "JobsCompleted",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "NextExecution",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TotalJobLimit",
                table: "Campaigns");
        }
    }
}
