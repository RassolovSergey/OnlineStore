using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class User_NormalizedEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "OrderItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "NormalizedEmail" = lower(trim("Email"))
                WHERE "NormalizedEmail" IS NULL OR length("NormalizedEmail") = 0;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
