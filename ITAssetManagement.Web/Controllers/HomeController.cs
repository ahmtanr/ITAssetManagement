using ITAssetManagement.Core;
using ITAssetManagement.Data;
using ITAssetManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManagement.Web.Controllers
{
    [Authorize] // Sadece giriş yapanlar görebilir
    public class HomeController : Controller
    {
        private static readonly string[] GrafikRenkleri = { "#1d4ed8", "#0ea5e9", "#f59e0b", "#ef4444", "#10b981", "#8b5cf6", "#64748b" };

        private readonly UygulamaDbContext _context;

        public HomeController(UygulamaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cihazlar = await _context.Demirbaslar.AsNoTracking().ToListAsync();
            var toplamCihaz = cihazlar.Count;

            var kategoriGruplari = cihazlar
                .GroupBy(d => string.IsNullOrWhiteSpace(d.Kategori) ? "Diğer" : d.Kategori)
                .OrderByDescending(g => g.Count())
                .Select((g, index) => new DurumDagilimi
                {
                    Durum = g.Key,
                    Adet = g.Count(),
                    Yuzde = toplamCihaz == 0 ? 0 : g.Count() * 100 / toplamCihaz,
                    Renk = GrafikRenkleri[index % GrafikRenkleri.Length]
                })
                .ToList();

            var model = new DashboardViewModel
            {
                ToplamCihaz = toplamCihaz,
                ZimmetliCihaz = cihazlar.Count(d => d.Durum == "Zimmetli"),
                BakimdakiCihaz = cihazlar.Count(d => d.Durum == "Bakımda"),
                ArizaliCihaz = cihazlar.Count(d => d.Durum == "Arızalı"),
                BekleyenTalep = await _context.Talepler.CountAsync(t => t.Durum == "Bekliyor"),
                KategoriDagilimi = kategoriGruplari,
                GrafikGradient = BuildGrafikGradient(kategoriGruplari),
                SonZimmetler = await _context.Zimmetler
                    .Include(z => z.Demirbas)
                    .Include(z => z.Kullanici)
                    .AsNoTracking()
                    .OrderByDescending(z => z.VerilisTarihi)
                    .Take(6)
                    .ToListAsync(),
                SonBakimlar = await _context.Bakimlar
                    .Include(b => b.Demirbas)
                    .AsNoTracking()
                    .OrderByDescending(b => b.BaslangicTarihi)
                    .Take(6)
                    .ToListAsync(),
                SonEklenenCihazlar = cihazlar
                    .OrderByDescending(d => d.AlimTarihi)
                    .Take(6)
                    .ToList()
            };

            return View(model);
        }

        private static string BuildGrafikGradient(List<DurumDagilimi> dagilim)
        {
            if (dagilim.Count == 0)
            {
                return "#e9ecef 0 100%";
            }

            var parcalar = new List<string>();
            double baslangic = 0;
            foreach (var d in dagilim)
            {
                var oran = d.Adet * 100.0 / dagilim.Sum(x => x.Adet);
                parcalar.Add($"{d.Renk} {baslangic:0.##}% {baslangic + oran:0.##}%");
                baslangic += oran;
            }

            return string.Join(", ", parcalar);
        }
    }
}