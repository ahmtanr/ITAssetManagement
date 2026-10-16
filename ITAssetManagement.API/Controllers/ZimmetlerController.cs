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
    public class ZimmetlerController : ControllerBase
    {
        private readonly UygulamaDbContext _context;

        public ZimmetlerController(UygulamaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetirTumZimmetler()
        {
            var zimmetler = await _context.Zimmetler
                .Include(z => z.Demirbas)
                .Include(z => z.Kullanici)
                .ToListAsync();
            return Ok(zimmetler);
        }

        // Talep Onaylandığında Otomatik Çalışacak Zimmetleme Metodu
        [HttpPost("Ata")]
        public async Task<IActionResult> ZimmetAta([FromBody] Zimmet yeniZimmet)
        {
            var bosCihaz = yeniZimmet.DemirbasId > 0
                ? await _context.Demirbaslar.FirstOrDefaultAsync(d => d.Id == yeniZimmet.DemirbasId && d.Durum == "Boşta")
                : await _context.Demirbaslar.FirstOrDefaultAsync(d => d.Durum == "Boşta");
            if (bosCihaz == null) return BadRequest("Seçilen cihaz boşta değil veya envanterde uygun cihaz bulunamadı!");

            yeniZimmet.DemirbasId = bosCihaz.Id;
            yeniZimmet.VerilisTarihi = DateTime.Now;
            yeniZimmet.AktifMi = true;

            // Cihaz durumunu "Zimmetli" yap
            bosCihaz.Durum = "Zimmetli";

            _context.Zimmetler.Add(yeniZimmet);
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Cihaz başarıyla zimmetlendi." });
        }

        // Zimmeti İade Alma (Zimmeti Sonlandırma / Loglama)
        [HttpPost("{id}/IadeAl")]
        public async Task<IActionResult> IadeAl(int id)
        {
            var zimmet = await _context.Zimmetler
                .Include(z => z.Demirbas)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (zimmet == null) return NotFound("Zimmet kaydı bulunamadı.");

            zimmet.AktifMi = false;
            zimmet.TeslimTarihi = DateTime.Now;

            if (zimmet.Demirbas != null)
            {
                zimmet.Demirbas.Durum = "Boşta";
            }

            await _context.SaveChangesAsync();
            return Ok(new { mesaj = "Cihaz iade alındı ve tekrar boşa çıkarıldı." });
        }
    }
}