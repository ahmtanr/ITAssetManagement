using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AyarlarEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ayarlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarkodZorunlu = table.Column<bool>(type: "bit", nullable: false),
                    YeniTalepBildirimiAcik = table.Column<bool>(type: "bit", nullable: false),
                    FirmaAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaAdres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaTelefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayarlar", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ayarlar");

            migrationBuilder.CreateTable(
                name: "UygulamaAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarkodZorunlu = table.Column<bool>(type: "bit", nullable: false),
                    BildirimlerAcik = table.Column<bool>(type: "bit", nullable: false),
                    FirmaAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaAdres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmaEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmaTelefon = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UygulamaAyarlari", x => x.Id);
                });
        }
    }
}
