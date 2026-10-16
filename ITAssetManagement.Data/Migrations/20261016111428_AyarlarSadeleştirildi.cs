using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AyarlarSadeleştirildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarkodZorunlu",
                table: "Ayarlar");

            migrationBuilder.DropColumn(
                name: "FirmaAdi",
                table: "Ayarlar");

            migrationBuilder.DropColumn(
                name: "FirmaAdres",
                table: "Ayarlar");

            migrationBuilder.DropColumn(
                name: "FirmaEmail",
                table: "Ayarlar");

            migrationBuilder.DropColumn(
                name: "YeniTalepBildirimiAcik",
                table: "Ayarlar");

            migrationBuilder.RenameColumn(
                name: "FirmaTelefon",
                table: "Ayarlar",
                newName: "Tema");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tema",
                table: "Ayarlar",
                newName: "FirmaTelefon");

            migrationBuilder.AddColumn<bool>(
                name: "BarkodZorunlu",
                table: "Ayarlar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FirmaAdi",
                table: "Ayarlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirmaAdres",
                table: "Ayarlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirmaEmail",
                table: "Ayarlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "YeniTalepBildirimiAcik",
                table: "Ayarlar",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
