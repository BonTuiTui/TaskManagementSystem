using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class TenMigrationMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Task_Task_id",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "Task_id",
                table: "Notification",
                newName: "TaskComment_id");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Notification",
                newName: "Notification_text");

            migrationBuilder.RenameColumn(
                name: "Notifications_id",
                table: "Notification",
                newName: "Notification_id");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_Task_id",
                table: "Notification",
                newName: "IX_Notification_TaskComment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_TaskComment_TaskComment_id",
                table: "Notification",
                column: "TaskComment_id",
                principalTable: "TaskComment",
                principalColumn: "Comment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_TaskComment_TaskComment_id",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "TaskComment_id",
                table: "Notification",
                newName: "Task_id");

            migrationBuilder.RenameColumn(
                name: "Notification_text",
                table: "Notification",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "Notification_id",
                table: "Notification",
                newName: "Notifications_id");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_TaskComment_id",
                table: "Notification",
                newName: "IX_Notification_Task_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Task_Task_id",
                table: "Notification",
                column: "Task_id",
                principalTable: "Task",
                principalColumn: "Task_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
