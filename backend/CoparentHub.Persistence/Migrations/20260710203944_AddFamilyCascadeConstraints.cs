using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoparentHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyCascadeConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_FamilyId",
                table: "Notifications",
                column: "FamilyId");

            // Remove any orphaned rows first — a FK can't be added while one exists.
            migrationBuilder.Sql(@"DELETE FROM ""Events"" e WHERE NOT EXISTS (SELECT 1 FROM ""Families"" f WHERE f.""Id"" = e.""FamilyId"");");
            migrationBuilder.Sql(@"DELETE FROM ""Expenses"" e WHERE NOT EXISTS (SELECT 1 FROM ""Families"" f WHERE f.""Id"" = e.""FamilyId"");");
            migrationBuilder.Sql(@"DELETE FROM ""Messages"" m WHERE NOT EXISTS (SELECT 1 FROM ""Families"" f WHERE f.""Id"" = m.""FamilyId"");");
            migrationBuilder.Sql(@"DELETE FROM ""Notifications"" n WHERE NOT EXISTS (SELECT 1 FROM ""Families"" f WHERE f.""Id"" = n.""FamilyId"");");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Families_FamilyId",
                table: "Events",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Families_FamilyId",
                table: "Expenses",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Families_FamilyId",
                table: "Messages",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Families_FamilyId",
                table: "Notifications",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Families_FamilyId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Families_FamilyId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Families_FamilyId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Families_FamilyId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_FamilyId",
                table: "Notifications");
        }
    }
}
