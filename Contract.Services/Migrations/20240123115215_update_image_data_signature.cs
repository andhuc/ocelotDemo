using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contract.Service.Migrations
{
    public partial class update_image_data_signature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "imageData",
                table: "signature",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "imageData",
                table: "signature",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
