namespace ITAssetManagement.Core
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public string SifreHash { get; set; } // Şifre doğrulaması için gerekli alan
        public string Rol { get; set; }
        public string AdSoyad { get; set; }
        public bool Onaylandi { get; set; }
    }
}