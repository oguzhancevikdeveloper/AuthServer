using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class userphonecodefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AspNetUserPhoneCodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AspNetUserPhoneCodes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
