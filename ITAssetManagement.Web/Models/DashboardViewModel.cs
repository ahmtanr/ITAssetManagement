using ITAssetManagement.Core;

namespace ITAssetManagement.Web.Models
{
    public class DashboardViewModel
    {
        public int ToplamCihaz { get; set; }
        public int ZimmetliCihaz { get; set; }
        public int BakimdakiCihaz { get; set; }
        public int ArizaliCihaz { get; set; }
        public int BekleyenTalep { get; set; }

        public List<DurumDagilimi> KategoriDagilimi { get; set; } = new();
        public string GrafikGradient { get; set; } = "#e9ecef 0 100%";

        public List<Zimmet> SonZimmetler { get; set; } = new();
        public List<Bakim> SonBakimlar { get; set; } = new();
        public List<Demirbas> SonEklenenCihazlar { get; set; } = new();
    }

    public class DurumDagilimi
    {
        public string Durum { get; set; }
        public int Adet { get; set; }
        public int Yuzde { get; set; }
        public string Renk { get; set; }
    }
}
