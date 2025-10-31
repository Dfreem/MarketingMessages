using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketingMessages.Migrations
{
    /// <inheritdoc />
    public partial class EditableAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JSONForm",
                table: "Audiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JSONForm",
                table: "Audiences");
        }
    }
}
