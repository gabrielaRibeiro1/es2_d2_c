using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESOF.WebApp.DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDBSchemaTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TalentProfiles_Users_user_id",
                table: "TalentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TalentProfiles_user_id",
                table: "TalentProfiles");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "TalentProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "TalentProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TalentProfiles_user_id",
                table: "TalentProfiles",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TalentProfiles_Users_user_id",
                table: "TalentProfiles",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id");
        }
    }
}
