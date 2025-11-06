using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfluenciAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePublicationsAddTenantUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "publications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "publications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_publications_TenantId_UserId",
                table: "publications",
                columns: new[] { "TenantId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_publications_TenantId_UserId",
                table: "publications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "publications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "publications");
        }
    }
}
