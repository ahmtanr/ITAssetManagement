using ITAssetManagement.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Web.Controllers
{
    public class TalepController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TalepController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // 1. Yöneticinin Gelen Talepleri Gördüğü ve Onayladığı Ekran
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Talepler");

            if (response.IsSuccessStatusCode)
            {
                var talepler = await response.Content.ReadFromJsonAsync<List<Talep>>() ?? new List<Talep>();
                return View(talepler.Where(t => t.KullaniciAdi == User.Identity!.Name).ToList());
            }

            return View(new List<Talep>());
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await LoadCihazlarAsync();
            return View(new Talep());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Talep talep)
        {
            talep.KullaniciAdi = User.Identity!.Name!;
            talep.Durum = "Bekliyor";

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsJsonAsync("Talepler", talep);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mesaj"] = "Talebiniz başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Talep gönderilirken bir hata oluştu.");
            await LoadCihazlarAsync();
            return View(talep);
        }

        private async Task LoadCihazlarAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Demirbaslar");

            ViewBag.Cihazlar = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Demirbas>>() ?? new List<Demirbas>()
                : new List<Demirbas>();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> YoneticiTalepleri()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Talepler"); // API'deki talep listesi uç noktası

            if (response.IsSuccessStatusCode)
            {
                var talepler = await response.Content.ReadFromJsonAsync<List<Talep>>();
                return View(talepler);
            }

            return View(new List<Talep>());
        }

        // 2. Talebi Onaylama ve Otomatik Zimmetleme İşlemi (POST)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OnaylaVeZimmetle(int talepId)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.PostAsync($"Talepler/{talepId}/OnaylaVeZimmetle", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Talep onaylandı ve boştaki uygun cihaz personele başarıyla zimmetlendi!";
            }
            else
            {
                var hataMesaji = (await response.Content.ReadAsStringAsync()).Trim('"');
                TempData["Hata"] = $"Talep onaylanırken bir hata oluştu: {hataMesaji}";
            }

            return RedirectToAction("YoneticiTalepleri");
        }
    }
}