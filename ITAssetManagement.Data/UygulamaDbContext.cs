using ITAssetManagement.Core;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManagement.Data
{
    public class UygulamaDbContext : DbContext
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Demirbas> Demirbaslar { get; set; }
        public DbSet<Talep> Talepler { get; set; }
        public DbSet<Zimmet> Zimmetler { get; set; }
        public DbSet<Bakim> Bakimlar { get; set; }
        public DbSet<Ayar> Ayarlar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Bakim>()
                .HasOne(b => b.Demirbas)
                .WithMany()
                .HasForeignKey(b => b.DemirbasId)
                .OnDelete(DeleteBehavior.Restrict);

            // İlişkilerde cascade delete (zincirleme silme) yerine kısıtlamalar koyabiliriz
            modelBuilder.Entity<Zimmet>()
                .HasOne(z => z.Demirbas)
                .WithMany()
                .HasForeignKey(z => z.DemirbasId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Zimmet>()
                .HasOne(z => z.Kullanici)
                .WithMany()
                .HasForeignKey(z => z.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}