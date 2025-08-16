using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CheckConstraints_Price_Order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "Projects",
                comment: "Таблица проектов, содержащая информацию об актуальных проектах, их моделях и изображениях.");

            migrationBuilder.AlterTable(
                name: "Models",
                comment: "Таблица моделей 3D, содержащая информацию о акутальных моделях, их проектах и изображениях.");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Price_NonNegative",
                table: "Projects",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItems_PriceAtPurchase_NonNegative",
                table: "OrderItems",
                sql: "\"PriceAtPurchase\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Models_Price_NonNegative",
                table: "Models",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ModelImages_Order_NonNegative",
                table: "ModelImages",
                sql: "\"Order\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_Price_NonNegative",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItems_PriceAtPurchase_NonNegative",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Models_Price_NonNegative",
                table: "Models");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ModelImages_Order_NonNegative",
                table: "ModelImages");

            migrationBuilder.AlterTable(
                name: "Projects",
                oldComment: "Таблица проектов, содержащая информацию об актуальных проектах, их моделях и изображениях.");

            migrationBuilder.AlterTable(
                name: "Models",
                oldComment: "Таблица моделей 3D, содержащая информацию о акутальных моделях, их проектах и изображениях.");
        }
    }
}
