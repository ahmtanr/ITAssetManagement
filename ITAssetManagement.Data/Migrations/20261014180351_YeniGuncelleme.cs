using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class YeniGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zimmetler_Demirbaslar_DemirbasId",
                table: "Zimmetler");

            migrationBuilder.DropForeignKey(
                name: "FK_Zimmetler_Kullanicilar_KullaniciId",
                table: "Zimmetler");

            migrationBuilder.AddColumn<string>(
                name: "AdSoyad",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sifre",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Zimmetler_Demirbaslar_DemirbasId",
                table: "Zimmetler",
                column: "DemirbasId",
                principalTable: "Demirbaslar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Zimmetler_Kullanicilar_KullaniciId",
                table: "Zimmetler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zimmetler_Demirbaslar_DemirbasId",
                table: "Zimmetler");

            migrationBuilder.DropForeignKey(
                name: "FK_Zimmetler_Kullanicilar_KullaniciId",
                table: "Zimmetler");

            migrationBuilder.DropColumn(
                name: "AdSoyad",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "Sifre",
                table: "Kullanicilar");

            migrationBuilder.AddForeignKey(
                name: "FK_Zimmetler_Demirbaslar_DemirbasId",
                table: "Zimmetler",
                column: "DemirbasId",
                principalTable: "Demirbaslar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Zimmetler_Kullanicilar_KullaniciId",
                table: "Zimmetler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
