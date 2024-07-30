using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class userphonecode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUserPhoneCodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneLoginCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAppId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPhoneCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserPhoneCodes_AspNetUsers_UserAppId",
                        column: x => x.UserAppId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPhoneCodes_UserAppId",
                table: "AspNetUserPhoneCodes",
                column: "UserAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserPhoneCodes");
        }
    }
}
