using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatGpt.Migrations
{
    /// <inheritdoc />
    public partial class ThreadOwnerAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Threads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Threads_OwnerId",
                table: "Threads",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_AspNetUsers_OwnerId",
                table: "Threads",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Threads_AspNetUsers_OwnerId",
                table: "Threads");

            migrationBuilder.DropIndex(
                name: "IX_Threads_OwnerId",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Threads");
        }
    }
}
