using ITAssetManagement.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Web.Controllers
{
    public class ZimmetController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ZimmetController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Zimmetli Cihazları Listeleme
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Zimmetler");

            if (response.IsSuccessStatusCode)
            {
                var zimmetler = await response.Content.ReadFromJsonAsync<List<Zimmet>>();
                return View(zimmetler);
            }

            return View(new List<Zimmet>());
        }

        // Personelin Üzerindeki Aktif Zimmetler
        [Authorize]
        public async Task<IActionResult> PersonelZimmetleri()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Zimmetler");

            if (response.IsSuccessStatusCode)
            {
                var zimmetler = await response.Content.ReadFromJsonAsync<List<Zimmet>>() ?? new List<Zimmet>();
                var kullaniciAdi = User.Identity!.Name;
                return View(zimmetler.Where(z => z.AktifMi && (z.Kullanici?.KullaniciAdi == kullaniciAdi || z.Kullanici?.AdSoyad == kullaniciAdi)).ToList());
            }

            return View(new List<Zimmet>());
        }

        // Zimmeti İade Alma (Cihazı Boşa Çıkarma ve Loglama)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IadeEt(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync($"Zimmetler/{id}/IadeAl", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Cihaz başarıyla iade alındı ve depoya (boşa) çekildi.";
            }
            else
            {
                TempData["Hata"] = "İade işlemi sırasında bir sorun oluştu.";
            }

            return RedirectToAction("Index");
        }

        // Yeni Zimmet (Atama) Formu
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await YukleListeleriAsync();
            return View(new Zimmet());
        }

        // Yeni Zimmet (Atama) Kaydı
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Zimmet yeniZimmet)
        {
            if (yeniZimmet.DemirbasId <= 0)
            {
                ModelState.AddModelError(nameof(Zimmet.DemirbasId), "Lütfen bir cihaz seçin.");
            }

            if (yeniZimmet.KullaniciId <= 0)
            {
                ModelState.AddModelError(nameof(Zimmet.KullaniciId), "Lütfen bir personel seçin.");
            }

            if (!ModelState.IsValid)
            {
                await YukleListeleriAsync();
                return View(yeniZimmet);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("Zimmetler/Ata", yeniZimmet);

            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Cihaz başarıyla zimmetlendi.";
                return RedirectToAction("Index");
            }

            var hataMesaji = (await response.Content.ReadAsStringAsync()).Trim('"');
            ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(hataMesaji)
                ? "Zimmet kaydedilemedi."
                : hataMesaji);

            await YukleListeleriAsync();
            return View(yeniZimmet);
        }

        private async Task YukleListeleriAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var cihazlar = await client.GetFromJsonAsync<List<Demirbas>>("Demirbaslar")
                           ?? new List<Demirbas>();
            var kullanicilar = await client.GetFromJsonAsync<List<Kullanici>>("Kullanicilar")
                               ?? new List<Kullanici>();

            ViewBag.Cihazlar = cihazlar.Where(c => c.Durum == "Boşta").ToList();
            ViewBag.Kullanicilar = kullanicilar.Where(k => k.Onaylandi).ToList();
        }
    }
}
