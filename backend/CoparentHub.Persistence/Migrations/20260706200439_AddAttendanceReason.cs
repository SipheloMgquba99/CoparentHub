using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoparentHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Attendances",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Attendances");
        }
    }
}
