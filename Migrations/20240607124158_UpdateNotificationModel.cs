using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_TaskComment_TaskComment_id",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_TaskComment_id",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "TaskComment_id",
                table: "Notification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskComment_id",
                table: "Notification",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_TaskComment_id",
                table: "Notification",
                column: "TaskComment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_TaskComment_TaskComment_id",
                table: "Notification",
                column: "TaskComment_id",
                principalTable: "TaskComment",
                principalColumn: "Comment_id");
        }
    }
}
