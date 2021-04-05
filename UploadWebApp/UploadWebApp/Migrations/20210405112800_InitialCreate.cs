using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UploadWebApp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContainsCyrillic = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContainsLatin = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContainsNumbers = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContainsSpChar = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileData", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileData");
        }
    }
}
