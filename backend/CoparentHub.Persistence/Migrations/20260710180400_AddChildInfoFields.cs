using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoparentHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChildInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClothingSize",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorPhone",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalNotes",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medications",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolContact",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "Children",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShoeSize",
                table: "Children",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "ClothingSize",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "DoctorPhone",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "MedicalNotes",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "Medications",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "SchoolContact",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "ShoeSize",
                table: "Children");
        }
    }
}
