using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatGpt.Migrations
{
    /// <inheritdoc />
    public partial class MessageTime_index_configuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_Time",
                table: "Messages",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_Time",
                table: "Messages");
        }
    }
}
