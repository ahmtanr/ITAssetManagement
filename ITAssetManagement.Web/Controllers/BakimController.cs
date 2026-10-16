using ITAssetManagement.Core;
using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BakimController : Controller
    {
        private static readonly string[] BakimDurumlari = { "Bakımda", "Arızalı" };

        private readonly UygulamaDbContext _context;

        public BakimController(UygulamaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var kayitlar = await _context.Bakimlar
                .Include(b => b.Demirbas)
                .AsNoTracking()
                .OrderByDescending(b => b.BaslangicTarihi)
                .ToListAsync();

            return View(kayitlar);
        }

        [HttpGet]
        public async Task<IActionResult> Olustur(int? demirbasId)
        {
            await SetDemirbaslarAsync(demirbasId);
            return View(new Bakim { DemirbasId = demirbasId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Bakim bakim, string durum)
        {
            ModelState.Remove(nameof(bakim.Demirbas));

            var demirbas = await _context.Demirbaslar.FindAsync(bakim.DemirbasId);
            if (demirbas == null)
            {
                ModelState.AddModelError(nameof(bakim.DemirbasId), "Cihaz bulunamadı.");
            }
            else if (demirbas.Durum == "Zimmetli")
            {
                ModelState.AddModelError(nameof(bakim.DemirbasId), "Zimmetli cihaz bakıma/arızaya alınamaz. Önce zimmeti iade edin.");
            }
            else if (await _context.Bakimlar.AnyAsync(b => b.DemirbasId == bakim.DemirbasId && b.AktifMi))
            {
                ModelState.AddModelError(nameof(bakim.DemirbasId), "Bu cihazda zaten açık bir bakım kaydı var.");
            }

            if (!BakimDurumlari.Contains(durum))
            {
                ModelState.AddModelError("durum", "Geçerli bir durum seçin.");
            }

            if (!ModelState.IsValid || demirbas == null)
            {
                await SetDemirbaslarAsync(bakim.DemirbasId);
                return View(bakim);
            }

            bakim.AktifMi = true;
            bakim.BaslangicTarihi = DateTime.Now;
            demirbas.Durum = durum;

            _context.Bakimlar.Add(bakim);
            await _context.SaveChangesAsync();

            TempData["Basarili"] = $"Bakım kaydı oluşturuldu. Cihaz durumu: {durum}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kapat(int id)
        {
            var bakim = await _context.Bakimlar.Include(b => b.Demirbas).FirstOrDefaultAsync(b => b.Id == id);
            if (bakim == null)
            {
                return NotFound();
            }

            if (bakim.AktifMi)
            {
                bakim.AktifMi = false;
                bakim.BitisTarihi = DateTime.Now;
                if (bakim.Demirbas != null)
                {
                    bakim.Demirbas.Durum = "Boşta";
                }
                await _context.SaveChangesAsync();
                TempData["Basarili"] = "Bakım kapatıldı, cihaz tekrar boşta.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task SetDemirbaslarAsync(int? secili)
        {
            var demirbaslar = await _context.Demirbaslar
                .AsNoTracking()
                .Where(d => d.Durum != "Zimmetli")
                .OrderBy(d => d.Marka)
                .ToListAsync();

            ViewBag.Demirbaslar = new SelectList(
                demirbaslar.Select(d => new { d.Id, Ad = $"{d.Marka} {d.Model} ({d.SeriNo})" }),
                "Id", "Ad", secili);
            ViewBag.Durumlar = new SelectList(BakimDurumlari);
        }
    }
}
