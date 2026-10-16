namespace ITAssetManagement.Core
{
    public class Talep
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; }
        public int KullaniciId { get; set; }
        public string CihazTuru { get; set; }
        public string Gerekce { get; set; }
        public DateTime TalepTarihi { get; set; } = DateTime.Now;
        public string Durum { get; set; } = "Bekliyor"; // "Bekliyor", "Onaylandı", "Reddedildi"
    }
}