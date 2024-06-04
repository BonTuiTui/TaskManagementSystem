using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Projects_Project_Id",
                table: "Task");

            migrationBuilder.RenameColumn(
                name: "Project_Id",
                table: "Task",
                newName: "Project_id");

            migrationBuilder.RenameIndex(
                name: "IX_Task_Project_Id",
                table: "Task",
                newName: "IX_Task_Project_id");

            migrationBuilder.RenameColumn(
                name: "Projects_id",
                table: "Projects",
                newName: "Project_id");

            migrationBuilder.AddColumn<int>(
                name: "Project_Id",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Task_Project_Id",
                table: "Task",
                column: "Project_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Projects_Project_Id",
                table: "Task",
                column: "Project_Id",
                principalTable: "Projects",
                principalColumn: "Project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Projects_Project_id",
                table: "Task",
                column: "Project_id",
                principalTable: "Projects",
                principalColumn: "Project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Projects_Project_Id",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Projects_Project_id",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_Project_Id",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Project_Id",
                table: "Task");

            migrationBuilder.RenameColumn(
                name: "Project_id",
                table: "Task",
                newName: "Project_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Task_Project_id",
                table: "Task",
                newName: "IX_Task_Project_Id");

            migrationBuilder.RenameColumn(
                name: "Project_id",
                table: "Projects",
                newName: "Projects_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Projects_Project_Id",
                table: "Task",
                column: "Project_Id",
                principalTable: "Projects",
                principalColumn: "Projects_id");
        }
    }
}
