using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfluenciAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationPropertiesAndMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_metric_snapshots_publications_PublicationId",
                table: "metric_snapshots",
                column: "PublicationId",
                principalTable: "publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_publications_contents_ContentId",
                table: "publications",
                column: "ContentId",
                principalTable: "contents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_publications_social_profiles_SocialProfileId",
                table: "publications",
                column: "SocialProfileId",
                principalTable: "social_profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_metric_snapshots_publications_PublicationId",
                table: "metric_snapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_publications_contents_ContentId",
                table: "publications");

            migrationBuilder.DropForeignKey(
                name: "FK_publications_social_profiles_SocialProfileId",
                table: "publications");
        }
    }
}
