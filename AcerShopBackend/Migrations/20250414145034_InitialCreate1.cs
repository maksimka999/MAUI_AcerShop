using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AcerShopBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "users",
                newName: "registration_date");

            migrationBuilder.RenameColumn(
                name: "FirebaseUid",
                table: "users",
                newName: "firebase_uid");

            migrationBuilder.RenameColumn(
                name: "CustomRole",
                table: "users",
                newName: "custom_role");

            migrationBuilder.AlterColumn<string>(
                name: "custom_role",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.product_id);
                });

            migrationBuilder.CreateTable(
                name: "chair_details",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    material = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    weight_capacity = table.Column<int>(type: "integer", nullable: true),
                    adjustable_features = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dimensions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    warranty_years = table.Column<int>(type: "integer", nullable: true),
                    comfort_rating = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chair_details", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_chair_details_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "laptop_details",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    processor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ram = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    storage_size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    screen_size = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    graphics_card = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    operating_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    battery_life = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    weight = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laptop_details", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_laptop_details_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mouse_details",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    dpi = table.Column<int>(type: "integer", nullable: true),
                    buttons = table.Column<int>(type: "integer", nullable: true),
                    connection_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ergonomic_design = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sensitivity_adjustment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    wireless_range = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    compatibility_platforms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mouse_details", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_mouse_details_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_cart",
                columns: table => new
                {
                    cart_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_cart", x => x.cart_id);
                    table.ForeignKey(
                        name: "FK_user_cart_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_cart_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_cart_product_id",
                table: "user_cart",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_cart_user_id",
                table: "user_cart",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chair_details");

            migrationBuilder.DropTable(
                name: "laptop_details");

            migrationBuilder.DropTable(
                name: "mouse_details");

            migrationBuilder.DropTable(
                name: "user_cart");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "registration_date",
                table: "Users",
                newName: "RegistrationDate");

            migrationBuilder.RenameColumn(
                name: "firebase_uid",
                table: "Users",
                newName: "FirebaseUid");

            migrationBuilder.RenameColumn(
                name: "custom_role",
                table: "Users",
                newName: "CustomRole");

            migrationBuilder.AlterColumn<string>(
                name: "CustomRole",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");
        }
    }
}
