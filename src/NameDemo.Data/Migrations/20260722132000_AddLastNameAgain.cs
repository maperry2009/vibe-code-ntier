using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NameDemo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastNameAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "guest_names",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastName",
                table: "guest_names");
        }
    }
}
