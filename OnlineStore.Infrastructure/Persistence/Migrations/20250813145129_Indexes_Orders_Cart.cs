using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Indexes_Orders_Cart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_Model3DId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProjectId",
                table: "OrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Model3DId",
                table: "OrderItems",
                column: "Model3DId",
                filter: "\"Model3DId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProjectId",
                table: "OrderItems",
                column: "ProjectId",
                filter: "\"ProjectId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Models_CreatedAt",
                table: "Models",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_AddedAt",
                table: "CartItems",
                columns: new[] { "CartId", "AddedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_Model3DId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProjectId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Models_CreatedAt",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_CartId_AddedAt",
                table: "CartItems");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_Model3DId",
                table: "OrderItems",
                column: "Model3DId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProjectId",
                table: "OrderItems",
                column: "ProjectId");
        }
    }
}
