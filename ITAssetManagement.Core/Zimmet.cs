namespace ITAssetManagement.Core
{
    public class Zimmet
    {
        public int Id { get; set; }
        public int DemirbasId { get; set; }
        public Demirbas Demirbas { get; set; }

        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }

        public DateTime VerilisTarihi { get; set; } = DateTime.Now;
        public DateTime? TeslimTarihi { get; set; } // İade edilirse dolacak
        public bool AktifMi { get; set; } = true;
    }
}