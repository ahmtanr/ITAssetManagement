using ITAssetManagement.Core;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ITAssetManagement.Web.Controllers
{
    public class FiltrelemeController : Controller
    {
        private readonly HttpClient _httpClient;

        public FiltrelemeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index(string durum)
        {
            var res = await _httpClient.GetAsync("Demirbaslar");
            var json = await res.Content.ReadAsStringAsync();
            var cihazlar = JsonSerializer.Deserialize<List<Demirbas>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Demirbas>();

            // Eğer filtre seçildiyse ona göre süz
            if (!string.IsNullOrEmpty(durum))
            {
                cihazlar = cihazlar.Where(c => c.Durum == durum).ToList();
            }

            ViewBag.SecilenDurum = durum;
            return View(cihazlar);
        }
    }
}