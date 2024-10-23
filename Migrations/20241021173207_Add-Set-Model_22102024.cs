using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyResource.Migrations
{
    /// <inheritdoc />
    public partial class AddSetModel_22102024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SetId",
                table: "Document",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Set",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Set", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_SetId",
                table: "Document",
                column: "SetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Set_SetId",
                table: "Document",
                column: "SetId",
                principalTable: "Set",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Set_SetId",
                table: "Document");

            migrationBuilder.DropTable(
                name: "Set");

            migrationBuilder.DropIndex(
                name: "IX_Document_SetId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "SetId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");
        }
    }
}
