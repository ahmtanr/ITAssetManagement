namespace ITAssetManagement.Core
{
    public class Demirbas
    {
        public int Id { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public string SeriNo { get; set; }
        public string? Barkod { get; set; }
        public string? Durum { get; set; }
        public string? Kategori { get; set; } = "Diğer";
        public DateTime AlimTarihi { get; set; } = DateTime.Now;
    }
}