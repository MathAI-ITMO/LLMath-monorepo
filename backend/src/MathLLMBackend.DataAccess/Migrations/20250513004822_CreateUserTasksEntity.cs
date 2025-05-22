using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathLLMBackend.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserTasksEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    ProblemId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssociatedChatId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTasks_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_ApplicationUserId",
                table: "UserTasks",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_ApplicationUserId_TaskType",
                table: "UserTasks",
                columns: new[] { "ApplicationUserId", "TaskType" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_AssociatedChatId",
                table: "UserTasks",
                column: "AssociatedChatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_ProblemId",
                table: "UserTasks",
                column: "ProblemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTasks");
        }
    }
}
