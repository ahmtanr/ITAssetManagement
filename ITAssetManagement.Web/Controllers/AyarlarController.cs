using ITAssetManagement.Core;
using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITAssetManagement.Web.Controllers
{
    [Authorize]
    public class AyarlarController : Controller
    {
        private readonly UygulamaDbContext _context;

        public AyarlarController(UygulamaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var kullanici = await GetAktifKullaniciAsync();
            if (kullanici == null)
            {
                return RedirectToAction("Index", "Giris");
            }

            ViewBag.Tema = await GetTemaAsync();
            ViewBag.Kullanici = kullanici;
            ViewBag.Surum = typeof(AyarlarController).Assembly.GetName().Version?.ToString() ?? "-";
            ViewBag.VeritabaniSunucu = _context.Database.GetDbConnection().DataSource;
            ViewBag.VeritabaniAdi = _context.Database.GetDbConnection().Database;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilGuncelle(string adSoyad, string kullaniciAdi)
        {
            var kullanici = await GetAktifKullaniciAsync();
            if (kullanici == null)
            {
                return RedirectToAction("Index", "Giris");
            }

            if (string.IsNullOrWhiteSpace(kullaniciAdi))
            {
                TempData["Hata"] = "Kullanıcı adı boş bırakılamaz.";
                return RedirectToAction(nameof(Index));
            }

            var ayniAd = await _context.Kullanicilar.AnyAsync(k => k.Id != kullanici.Id && k.KullaniciAdi.ToLower() == kullaniciAdi.Trim().ToLower());
            if (ayniAd)
            {
                TempData["Hata"] = "Bu kullanıcı adı zaten alınmış.";
                return RedirectToAction(nameof(Index));
            }

            kullanici.AdSoyad = adSoyad?.Trim() ?? string.Empty;
            kullanici.KullaniciAdi = kullaniciAdi.Trim();
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(kullanici.AdSoyad) ? kullanici.KullaniciAdi : kullanici.AdSoyad),
                new Claim(ClaimTypes.Role, kullanici.Rol ?? "Personel")
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

            TempData["Basarili"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(string mevcutSifre, string yeniSifre, string yeniSifreTekrar)
        {
            var kullanici = await GetAktifKullaniciAsync();
            if (kullanici == null)
            {
                return RedirectToAction("Index", "Giris");
            }

            var hasher = new PasswordHasher<Kullanici>();
            if (string.IsNullOrEmpty(kullanici.SifreHash) ||
                hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, mevcutSifre ?? string.Empty) == PasswordVerificationResult.Failed)
            {
                TempData["Hata"] = "Mevcut şifre hatalı.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(yeniSifre) || yeniSifre.Length < 6)
            {
                TempData["Hata"] = "Yeni şifre en az 6 karakter olmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (yeniSifre != yeniSifreTekrar)
            {
                TempData["Hata"] = "Yeni şifreler birbiriyle eşleşmiyor.";
                return RedirectToAction(nameof(Index));
            }

            kullanici.SifreHash = hasher.HashPassword(kullanici, yeniSifre);
            await _context.SaveChangesAsync();

            TempData["Basarili"] = "Şifreniz değiştirildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TemaKaydet(string tema)
        {
            var secim = tema == "Koyu" ? "Koyu" : "Acik";
            var ayar = await _context.Ayarlar.FirstOrDefaultAsync();
            if (ayar == null)
            {
                _context.Ayarlar.Add(new Ayar { Tema = secim });
            }
            else
            {
                ayar.Tema = secim;
            }

            await _context.SaveChangesAsync();

            TempData["Basarili"] = "Tema tercihiniz kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GetTemaAsync()
        {
            var ayar = await _context.Ayarlar.AsNoTracking().FirstOrDefaultAsync();
            return ayar?.Tema ?? "Acik";
        }

        private async Task<Kullanici?> GetAktifKullaniciAsync()
        {
            var kimlik = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(kimlik, out var id))
            {
                return null;
            }

            return await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == id);
        }
    }
}
