using ITAssetManagement.Core;
using ITAssetManagement.Data; // DbContext için gerekli
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ITAssetManagement.Web.Controllers
{
    public class ApiTokenYaniti
    {
        public string Token { get; set; } = string.Empty;
    }

    public class GirisController : Controller
    {
        private readonly UygulamaDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public GirisController(UygulamaDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Giris/Index
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Giris/Index
        [HttpPost]
        public async Task<IActionResult> Index(string kullaniciAdi, string sifre, string girisTuru)
        {
            var bulunanKullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi != null && k.KullaniciAdi.ToLower() == kullaniciAdi.Trim().ToLower());

            if (bulunanKullanici != null)
            {
                if (string.IsNullOrEmpty(sifre) || string.IsNullOrEmpty(bulunanKullanici.SifreHash))
                {
                    ViewBag.Hata = "Kullanıcı adı veya şifre hatalı!";
                    return View();
                }

                var hasher = new PasswordHasher<Kullanici>();
                var verify = hasher.VerifyHashedPassword(bulunanKullanici, bulunanKullanici.SifreHash, sifre);

                if (verify != PasswordVerificationResult.Success)
                {
                    ViewBag.Hata = "Kullanıcı adı veya şifre hatalı!";
                    return View();
                }

                if (!bulunanKullanici.Onaylandi && bulunanKullanici.Rol != "Admin")
                {
                    ViewBag.Hata = "Hesabınız henüz yönetici tarafından onaylanmadı.";
                    return View();
                }

                var gercekRol = string.IsNullOrEmpty(bulunanKullanici.Rol) ? "Personel" : bulunanKullanici.Rol;

                if (girisTuru == "Admin" && gercekRol != "Admin")
                {
                    ViewBag.Hata = "Bu hesap yönetici yetkisine sahip değil!";
                    return View();
                }

                if (girisTuru == "Personel" && gercekRol == "Admin")
                {
                    ViewBag.Hata = "Yöneticiler kullanıcı girişinden giremez!";
                    return View();
                }

                var client = _httpClientFactory.CreateClient("ApiClient");
                var tokenYaniti = await client.PostAsJsonAsync("Auth/Token", new { kullaniciAdi, sifre });

                if (!tokenYaniti.IsSuccessStatusCode)
                {
                    ViewBag.Hata = "Kimlik doğrulama servisine ulaşılamadı veya giriş reddedildi.";
                    return View();
                }

                var tokenVerisi = await tokenYaniti.Content.ReadFromJsonAsync<ApiTokenYaniti>();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, bulunanKullanici.Id.ToString()),
                    new Claim(ClaimTypes.Name, bulunanKullanici.AdSoyad ?? bulunanKullanici.KullaniciAdi),
                    new Claim(ClaimTypes.Role, gercekRol),
                    new Claim("ApiToken", tokenVerisi!.Token)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hata = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // GET: /Giris/KayitOl
        public IActionResult KayitOl()
        {
            return View();
        }

        // POST: /Giris/KayitOl
        [HttpPost]
        public async Task<IActionResult> KayitOl(Kullanici yeniKullanici)
        {
            if (string.IsNullOrEmpty(yeniKullanici.KullaniciAdi) || string.IsNullOrEmpty(yeniKullanici.Sifre))
            {
                ViewBag.Hata = "Tüm alanları doldurmanız gerekmektedir.";
                return View(yeniKullanici);
            }

            // Aynı kullanıcı adı var mı kontrolü
            var varMi = await _context.Kullanicilar.AnyAsync(k => k.KullaniciAdi.ToLower() == yeniKullanici.KullaniciAdi.ToLower());
            if (varMi)
            {
                ViewBag.Hata = "Bu kullanıcı adı zaten alınmış!";
                return View(yeniKullanici);
            }

            yeniKullanici.Rol = "Personel";
            yeniKullanici.Onaylandi = false;

            var hasher = new PasswordHasher<Kullanici>();
            yeniKullanici.SifreHash = hasher.HashPassword(yeniKullanici, yeniKullanici.Sifre);

            _context.Kullanicilar.Add(yeniKullanici);
            await _context.SaveChangesAsync();

            TempData["BasariliMesaj"] = "Kayıt talebiniz yöneticiye iletildi. Onaylandıktan sonra giriş yapabilirsiniz.";
            return RedirectToAction("Index", "Giris");
        }

        // Çıkış Yap
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}