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
    public class DemirbaslarController : ControllerBase
    {
        private readonly UygulamaDbContext _context;

        public DemirbaslarController(UygulamaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetirTumDemirbaslar()
        {
            var demirbaslar = await _context.Demirbaslar.ToListAsync();
            return Ok(demirbaslar);
        }

        [HttpGet("Barkod/{barkod}")]
        public async Task<IActionResult> GetirBarkodaGore(string barkod)
        {
            var demirbas = await _context.Demirbaslar.FirstOrDefaultAsync(d => d.Barkod == barkod);
            if (demirbas == null) return NotFound("Cihaz bulunamadı.");
            return Ok(demirbas);
        }

        [HttpPost]
        public async Task<IActionResult> DemirbasEkle(Demirbas yeniDemirbas)
        {
            // Otomatik kurumsal Barkod üretimi (IT-Yıl-Rastgele6Hane)
            if (string.IsNullOrEmpty(yeniDemirbas.Barkod))
            {
                yeniDemirbas.Barkod = $"IT-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            }

            if (string.IsNullOrEmpty(yeniDemirbas.Durum))
            {
                yeniDemirbas.Durum = "Boşta";
            }

            _context.Demirbaslar.Add(yeniDemirbas);
            await _context.SaveChangesAsync();

            return Ok(yeniDemirbas);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DemirbasSil(int id)
        {
            var demirbas = await _context.Demirbaslar.FindAsync(id);
            if (demirbas == null) return NotFound();

            _context.Demirbaslar.Remove(demirbas);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}/ArizaIsaretle")]
        public async Task<IActionResult> ArizaIsaretle(int id)
        {
            var demirbas = await _context.Demirbaslar.FindAsync(id);
            if (demirbas == null) return NotFound("Cihaz bulunamadı.");
            if (demirbas.Durum == "Zimmetli") return BadRequest("Zimmetli cihaz arızalı olarak işaretlenemez. Önce zimmeti iade alın.");

            demirbas.Durum = "Arızalı";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/ArizaGider")]
        public async Task<IActionResult> ArizaGider(int id)
        {
            var demirbas = await _context.Demirbaslar.FindAsync(id);
            if (demirbas == null) return NotFound("Cihaz bulunamadı.");
            if (demirbas.Durum != "Arızalı") return BadRequest("Cihaz arızalı durumda değil.");

            demirbas.Durum = "Boşta";
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}