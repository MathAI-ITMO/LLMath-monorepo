using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathLLMBackend.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSystemPromptToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemPrompt",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystemPrompt",
                table: "Messages");
        }
    }
}
