using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcerShopBackend.Migrations
{
    /// <inheritdoc />
    public partial class photoAsByte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chair_details_products_product_id",
                table: "chair_details");

            migrationBuilder.DropForeignKey(
                name: "FK_laptop_details_products_product_id",
                table: "laptop_details");

            migrationBuilder.DropForeignKey(
                name: "FK_mouse_details_products_product_id",
                table: "mouse_details");

            migrationBuilder.DropForeignKey(
                name: "FK_user_cart_products_product_id",
                table: "user_cart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_products",
                table: "products");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "products");

            migrationBuilder.RenameTable(
                name: "products",
                newName: "Products");

            migrationBuilder.AddColumn<byte[]>(
                name: "photo",
                table: "Products",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_chair_details_Products_product_id",
                table: "chair_details",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_laptop_details_Products_product_id",
                table: "laptop_details",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mouse_details_Products_product_id",
                table: "mouse_details",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_cart_Products_product_id",
                table: "user_cart",
                column: "product_id",
                principalTable: "Products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chair_details_Products_product_id",
                table: "chair_details");

            migrationBuilder.DropForeignKey(
                name: "FK_laptop_details_Products_product_id",
                table: "laptop_details");

            migrationBuilder.DropForeignKey(
                name: "FK_mouse_details_Products_product_id",
                table: "mouse_details");

            migrationBuilder.DropForeignKey(
                name: "FK_user_cart_Products_product_id",
                table: "user_cart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "photo",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "products");

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_products",
                table: "products",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_chair_details_products_product_id",
                table: "chair_details",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_laptop_details_products_product_id",
                table: "laptop_details",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mouse_details_products_product_id",
                table: "mouse_details",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_cart_products_product_id",
                table: "user_cart",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
