using ITAssetManagement.Core;
using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaleplerController : ControllerBase
    {
        private readonly UygulamaDbContext _context;

        public TaleplerController(UygulamaDbContext context)
        {
            _context = context;
        }

        // Tüm talepleri getirir (Yönetici görecek)
        [HttpGet]
        public async Task<IActionResult> GetTalepler()
        {
            return Ok(await _context.Talepler.ToListAsync());
        }

        // Yeni talep kaydeder (Personel gönderecek)
        [HttpPost]
        public async Task<IActionResult> PostTalep(Talep talep)
        {
            var talepEden = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi == talep.KullaniciAdi || k.AdSoyad == talep.KullaniciAdi);
            if (talepEden == null) return BadRequest("Talep eden kullanıcı sistemde bulunamadı.");
            talep.KullaniciId = talepEden.Id;

            _context.Talepler.Add(talep);
            await _context.SaveChangesAsync();
            return Ok(talep);
        }
        // Talebi Onayla ve Otomatik Zimmetle
        // Talebi Onayla ve Otomatik Zimmetle
        // Talebi Onayla ve Otomatik Zimmetle
        // Talebi Onayla ve Otomatik Zimmetle
        [HttpPost("{id}/OnaylaVeZimmetle")]
        public async Task<IActionResult> OnaylaVeZimmetle(int id)
        {
            var talep = await _context.Talepler.FindAsync(id);
            if (talep == null) return NotFound("Talep bulunamadı.");

            talep.Durum = "Onaylandı";

            // 1. Talebi yapan kullanıcıyı buluyoruz
            var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == talep.KullaniciId || k.KullaniciAdi == talep.KullaniciAdi || k.AdSoyad == talep.KullaniciAdi);
            if (kullanici == null) return BadRequest("Talep eden kullanıcı sistemde bulunamadı.");
            talep.KullaniciId = kullanici.Id;

            // 2. Personelin talep formunda seçtiği cihaz adına birebir uyan demirbaşı envanterden buluyoruz
            // Not: Demirbas modelindeki özellik isimlerin (Marka, Model vb.) nasıl tutulduğuna göre burayı ayarlayabiliriz.
            var demirbas = await _context.Demirbaslar
                .FirstOrDefaultAsync(d => d.Durum == "Boşta" && ((d.Marka + " " + d.Model) == talep.CihazTuru || d.Marka == talep.CihazTuru));

            // Eğer tam eşleşen boşta cihaz bulunamazsa, boşta olan ilk cihazı alalım
            if (demirbas == null)
            {
                demirbas = await _context.Demirbaslar.FirstOrDefaultAsync(d => d.Durum == "Boşta");
                if (demirbas == null) return BadRequest("Depoda boşta cihaz bulunamadı.");
            }

            // 3. Doğru cihaz ve doğru kullanıcı ID'si ile yeni zimmet kaydını oluşturuyoruz
            var yeniZimmet = new Zimmet
            {
                DemirbasId = demirbas.Id,
                KullaniciId = kullanici.Id,
                VerilisTarihi = DateTime.Now,
                AktifMi = true
            };

            // Cihazın durumunu "Zimmetli" yaparak aynı cihazın tekrar zimmetlenmesini engelliyoruz
            demirbas.Durum = "Zimmetli";

            _context.Zimmetler.Add(yeniZimmet);
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Talep onaylandı ve seçilen cihaz başarıyla zimmetlendi!" });
        }
    }
}