using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcerShopBackend.Migrations
{
    /// <inheritdoc />
    public partial class addUserField2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "users");
        }
    }
}
