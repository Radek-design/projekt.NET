using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projekt.NET.Migrations
{
    /// <inheritdoc />
    public partial class AddPremieres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumPost_AspNetUsers_UserId",
                table: "ForumPost");

            migrationBuilder.DropForeignKey(
                name: "FK_Screenshot_AspNetUsers_UserId",
                table: "Screenshot");

            migrationBuilder.DropForeignKey(
                name: "FK_Screenshot_Games_GameId",
                table: "Screenshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Screenshot",
                table: "Screenshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumPost",
                table: "ForumPost");

            migrationBuilder.RenameTable(
                name: "Screenshot",
                newName: "Screenshots");

            migrationBuilder.RenameTable(
                name: "ForumPost",
                newName: "ForumPosts");

            migrationBuilder.RenameIndex(
                name: "IX_Screenshot_UserId",
                table: "Screenshots",
                newName: "IX_Screenshots_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Screenshot_GameId",
                table: "Screenshots",
                newName: "IX_Screenshots_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_ForumPost_UserId",
                table: "ForumPosts",
                newName: "IX_ForumPosts_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Screenshots",
                table: "Screenshots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumPosts",
                table: "ForumPosts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Premieres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Premieres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Premieres_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Premieres_GameId",
                table: "Premieres",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumPosts_AspNetUsers_UserId",
                table: "ForumPosts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Screenshots_AspNetUsers_UserId",
                table: "Screenshots",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenshots_Games_GameId",
                table: "Screenshots",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumPosts_AspNetUsers_UserId",
                table: "ForumPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_Screenshots_AspNetUsers_UserId",
                table: "Screenshots");

            migrationBuilder.DropForeignKey(
                name: "FK_Screenshots_Games_GameId",
                table: "Screenshots");

            migrationBuilder.DropTable(
                name: "Premieres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Screenshots",
                table: "Screenshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumPosts",
                table: "ForumPosts");

            migrationBuilder.RenameTable(
                name: "Screenshots",
                newName: "Screenshot");

            migrationBuilder.RenameTable(
                name: "ForumPosts",
                newName: "ForumPost");

            migrationBuilder.RenameIndex(
                name: "IX_Screenshots_UserId",
                table: "Screenshot",
                newName: "IX_Screenshot_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Screenshots_GameId",
                table: "Screenshot",
                newName: "IX_Screenshot_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_ForumPosts_UserId",
                table: "ForumPost",
                newName: "IX_ForumPost_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Screenshot",
                table: "Screenshot",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumPost",
                table: "ForumPost",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumPost_AspNetUsers_UserId",
                table: "ForumPost",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Screenshot_AspNetUsers_UserId",
                table: "Screenshot",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenshot_Games_GameId",
                table: "Screenshot",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
