namespace ITAssetManagement.Core
{
    public class Bakim
    {
        public int Id { get; set; }
        public int DemirbasId { get; set; }
        public Demirbas Demirbas { get; set; }

        public string Aciklama { get; set; }
        public DateTime BaslangicTarihi { get; set; } = DateTime.Now;
        public DateTime? BitisTarihi { get; set; }
        public bool AktifMi { get; set; } = true;
    }
}
