using ITAssetManagement.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization; 

namespace ITAssetManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KullaniciController : Controller
    {
        private readonly HttpClient _httpClient;

        public KullaniciController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index()
        {
            // API'den tüm kullanıcıları çekiyoruz
            var response = await _httpClient.GetAsync("Kullanicilar");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var kullanicilar = JsonSerializer.Deserialize<List<Kullanici>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(kullanicilar);
            }
            return View(new List<Kullanici>());
        }
        // GET: Yeni Personel Ekleme Formu
        public IActionResult Create()
        {
            return View();
        }

        // POST: Formdan gelen veriyi API'ye gönderip kaydetme
        [HttpPost]
        public async Task<IActionResult> Create(Kullanici yeniKullanici)
        {
            var response = await _httpClient.PostAsJsonAsync("Kullanicilar", yeniKullanici);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index"); // Başarılıysa listeye geri dön
            }

            return View(yeniKullanici); // Hata varsa formu tekrar göster
        }

        // GET: Kullan\u0131c\u0131 D\u00FCzenleme Formu
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"Kullanicilar/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Hata"] = "Personel bulunamad\u0131.";
                return RedirectToAction(nameof(Index));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var kullanici = JsonSerializer.Deserialize<Kullanici>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            kullanici.Sifre = string.Empty;
            return View(kullanici);
        }

        // POST: D\u00FCzenleme kayd\u0131. Sifre bo\u015F b\u0131rak\u0131l\u0131rsa mevcut \u015fifre korunur.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kullanici guncellenen)
        {
            if (string.IsNullOrWhiteSpace(guncellenen.KullaniciAdi))
            {
                ModelState.AddModelError(nameof(Kullanici.KullaniciAdi), "Kullan\u0131c\u0131 ad\u0131 zorunludur.");
            }

            if (!string.IsNullOrEmpty(guncellenen.Sifre) && guncellenen.Sifre.Length < 6)
            {
                ModelState.AddModelError(nameof(Kullanici.Sifre), "\u015Eifre en az 6 karakter olmal\u0131d\u0131r.");
            }

            if (!ModelState.IsValid)
            {
                return View(guncellenen);
            }

            guncellenen.Id = id;
            var response = await _httpClient.PutAsJsonAsync($"Kullanicilar/{id}", guncellenen);
            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Personel bilgileri g\u00FCncellendi.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Personel g\u00FCncellenemedi.");
            return View(guncellenen);
        }

        [HttpPost]
        public async Task<IActionResult> Onayla(int id)
        {
            var response = await _httpClient.PostAsync($"Kullanicilar/{id}/Onayla", null);
            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Personel hesabı onaylandı.";
            }
            else
            {
                TempData["Hata"] = "Personel onaylanamadı.";
            }
            return RedirectToAction("Index");
        }

        // GET: Personel Silme İşlemi
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"Kullanicilar/{id}");
            return RedirectToAction("Index"); // Sildikten sonra listeye dön
        }
    }
}
