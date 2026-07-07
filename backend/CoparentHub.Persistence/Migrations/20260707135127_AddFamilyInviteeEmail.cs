using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoparentHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyInviteeEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteeEmail",
                table: "FamilyInvites",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyInvites_InviteeEmail",
                table: "FamilyInvites",
                column: "InviteeEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FamilyInvites_InviteeEmail",
                table: "FamilyInvites");

            migrationBuilder.DropColumn(
                name: "InviteeEmail",
                table: "FamilyInvites");
        }
    }
}
