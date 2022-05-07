using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Web.Data.Migrations
{
    public partial class CascadeDeleteForFileOnFileSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileOnFileSystem_Idea_IdeaId",
                table: "FileOnFileSystem");

            migrationBuilder.AlterColumn<int>(
                name: "IdeaId",
                table: "FileOnFileSystem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FileOnFileSystem_Idea_IdeaId",
                table: "FileOnFileSystem",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileOnFileSystem_Idea_IdeaId",
                table: "FileOnFileSystem");

            migrationBuilder.AlterColumn<int>(
                name: "IdeaId",
                table: "FileOnFileSystem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FileOnFileSystem_Idea_IdeaId",
                table: "FileOnFileSystem",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "Id");
        }
    }
}
