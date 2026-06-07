using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projekt.NET.Migrations
{
    /// <inheritdoc />
    public partial class RozdzieleniePremierGier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Premieres_Games_GameId",
                table: "Premieres");

            migrationBuilder.DropIndex(
                name: "IX_Premieres_GameId",
                table: "Premieres");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Premieres");

            migrationBuilder.AddColumn<string>(
                name: "Genres",
                table: "Premieres",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Platforms",
                table: "Premieres",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Premieres",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Premieres",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genres",
                table: "Premieres");

            migrationBuilder.DropColumn(
                name: "Platforms",
                table: "Premieres");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Premieres");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Premieres");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Premieres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Premieres_GameId",
                table: "Premieres",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Premieres_Games_GameId",
                table: "Premieres",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
