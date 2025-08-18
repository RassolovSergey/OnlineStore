using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    company_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.CheckConstraint("CK_Projects_Price_NonNegative", "price >= 0");
                },
                comment: "Проекты (наборы моделей) и их метаданные.");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                },
                comment: "Пользователи интернет-магазина.");

            migrationBuilder.CreateTable(
                name: "models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    company_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_models", x => x.id);
                    table.CheckConstraint("CK_Models_Price_NonNegative", "price >= 0");
                    table.ForeignKey(
                        name: "fk_models_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "3D-модели и их метаданные.");

            migrationBuilder.CreateTable(
                name: "project_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_preview = table.Column<bool>(type: "boolean", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_images", x => x.id);
                    table.CheckConstraint("CK_ProjectImages_Order_NonNegative", "\"order\" >= 0");
                    table.ForeignKey(
                        name: "fk_project_images_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Изображения, привязанные к проектам.");

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carts", x => x.id);
                    table.ForeignKey(
                        name: "fk_carts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Корзины пользователей.");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Заказы пользователей.");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_by_ua = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Refresh-токены пользователей (храним хеш).");

            migrationBuilder.CreateTable(
                name: "model_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_preview = table.Column<bool>(type: "boolean", nullable: false),
                    model3d_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_model_images", x => x.id);
                    table.CheckConstraint("CK_ModelImages_Order_NonNegative", "\"order\" >= 0");
                    table.ForeignKey(
                        name: "fk_model_images_models_model3d_id",
                        column: x => x.model3d_id,
                        principalTable: "models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Изображения, привязанные к 3D-моделям.");

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model3d_id = table.Column<Guid>(type: "uuid", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                    table.CheckConstraint("CK_CartItem_ExactlyOneRef", "(CASE WHEN model3d_id IS NOT NULL THEN project_id IS NULL WHEN project_id IS NOT NULL THEN model3d_id IS NULL ELSE FALSE END)");
                    table.ForeignKey(
                        name: "fk_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cart_items_models_model3d_id",
                        column: x => x.model3d_id,
                        principalTable: "models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cart_items_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Позиции в корзинах (модель или проект).");

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model3d_id = table.Column<Guid>(type: "uuid", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    price_at_purchase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.CheckConstraint("CK_OrderItem_ExactlyOneRef", "(CASE WHEN model3d_id IS NOT NULL THEN project_id IS NULL WHEN project_id IS NOT NULL THEN model3d_id IS NULL ELSE FALSE END)");
                    table.CheckConstraint("CK_OrderItem_PriceAtPurchase_NonNegative", "price_at_purchase >= 0");
                    table.ForeignKey(
                        name: "fk_order_items_models_model3d_id",
                        column: x => x.model3d_id,
                        principalTable: "models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_items_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Позиции заказа: либо модель, либо проект.");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_added_at",
                table: "cart_items",
                columns: new[] { "cart_id", "added_at" });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_model3d_id",
                table: "cart_items",
                columns: new[] { "cart_id", "model3d_id" },
                unique: true,
                filter: "model3d_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_project_id",
                table: "cart_items",
                columns: new[] { "cart_id", "project_id" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_model3d_id",
                table: "cart_items",
                column: "model3d_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_project_id",
                table: "cart_items",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_carts_user_id",
                table: "carts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_model_images_model3d_id",
                table: "model_images",
                column: "model3d_id",
                unique: true,
                filter: "is_preview = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_model_images_model3d_id_order",
                table: "model_images",
                columns: new[] { "model3d_id", "order" });

            migrationBuilder.CreateIndex(
                name: "ix_models_created_at",
                table: "models",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_models_project_id",
                table: "models",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_model3d_id",
                table: "order_items",
                column: "model3d_id",
                filter: "model3d_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id_model3d_id",
                table: "order_items",
                columns: new[] { "order_id", "model3d_id" },
                unique: true,
                filter: "model3d_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id_project_id",
                table: "order_items",
                columns: new[] { "order_id", "project_id" },
                unique: true,
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_project_id",
                table: "order_items",
                column: "project_id",
                filter: "project_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id_created_at",
                table: "orders",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_project_images_project_id",
                table: "project_images",
                column: "project_id",
                unique: true,
                filter: "is_preview = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_project_images_project_id_order",
                table: "project_images",
                columns: new[] { "project_id", "order" });

            migrationBuilder.CreateIndex(
                name: "ix_projects_created_at",
                table: "projects",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_normalized_email",
                table: "users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "model_images");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "project_images");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "models");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
