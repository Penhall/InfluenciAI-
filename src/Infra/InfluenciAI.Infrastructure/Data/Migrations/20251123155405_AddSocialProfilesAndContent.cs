using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfluenciAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialProfilesAndContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "social_profiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "contents",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "SocialProfileId",
                table: "contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_contents_SocialProfileId",
                table: "contents",
                column: "SocialProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_contents_social_profiles_SocialProfileId",
                table: "contents",
                column: "SocialProfileId",
                principalTable: "social_profiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_contents_tenants_TenantId",
                table: "contents",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_contents_AspNetUsers_UserId",
                table: "contents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_social_profiles_tenants_TenantId",
                table: "social_profiles",
                column: "TenantId",
                principalTable: "tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_social_profiles_AspNetUsers_UserId",
                table: "social_profiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contents_social_profiles_SocialProfileId",
                table: "contents");

            migrationBuilder.DropForeignKey(
                name: "FK_contents_tenants_TenantId",
                table: "contents");

            migrationBuilder.DropForeignKey(
                name: "FK_contents_AspNetUsers_UserId",
                table: "contents");

            migrationBuilder.DropForeignKey(
                name: "FK_social_profiles_tenants_TenantId",
                table: "social_profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_social_profiles_AspNetUsers_UserId",
                table: "social_profiles");

            migrationBuilder.DropIndex(
                name: "IX_contents_SocialProfileId",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "SocialProfileId",
                table: "contents");

            migrationBuilder.RenameColumn(
                name: "Retweets",
                table: "metric_snapshots",
                newName: "Shares");

            migrationBuilder.RenameColumn(
                name: "Replies",
                table: "metric_snapshots",
                newName: "Impressions");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "social_profiles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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

            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "metric_snapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Comments",
                table: "metric_snapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "contents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_publications_TenantId_UserId",
                table: "publications",
                columns: new[] { "TenantId", "UserId" });
        }
    }
}
