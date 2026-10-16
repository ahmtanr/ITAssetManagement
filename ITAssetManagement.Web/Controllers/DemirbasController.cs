using ITAssetManagement.Core;
using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ITAssetManagement.Web.Controllers
{
    [Authorize]
    public class DemirbasController : Controller
    {
        private static readonly string[] KategoriListesi = { "Bilgisayar", "Laptop", "Monitör", "Yazıcı", "Telefon", "Diğer" };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UygulamaDbContext _context;

        public DemirbasController(IHttpClientFactory httpClientFactory, UygulamaDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        private void SetKategoriler()
        {
            ViewBag.Kategoriler = new SelectList(KategoriListesi);
        }

        // 1. Cihaz Listesi
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("Demirbaslar");

            List<Demirbas> liste = new List<Demirbas>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                liste = JsonSerializer.Deserialize<List<Demirbas>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Demirbas>();
            }

            return View(liste);
        }

        // 2. Yeni Cihaz Ekleme Sayfası (GET)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            SetKategoriler();
            return View(new Demirbas());
        }

        // 3. Yeni Cihaz Kaydetme İşlemi (POST)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Demirbas yeniDemirbas)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // API'ye gönderiyoruz
            var response = await client.PostAsJsonAsync("Demirbaslar", yeniDemirbas);

            if (response.IsSuccessStatusCode)
            {
                TempData["BasariliMesaj"] = "Cihaz başarıyla eklendi!";
                return RedirectToAction("Index");
            }

            ViewBag.Hata = "Cihaz eklenirken bir hata oluştu!";
            SetKategoriler();
            return View(yeniDemirbas);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var demirbas = await _context.Demirbaslar.FindAsync(id);
            if (demirbas == null)
            {
                return NotFound();
            }

            SetKategoriler();
            return View(demirbas);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Demirbas guncel)
        {
            var demirbas = await _context.Demirbaslar.FindAsync(guncel.Id);
            if (demirbas == null)
            {
                return NotFound();
            }

            demirbas.Marka = guncel.Marka;
            demirbas.Model = guncel.Model;
            demirbas.SeriNo = guncel.SeriNo;
            demirbas.Kategori = guncel.Kategori;
            await _context.SaveChangesAsync();

            TempData["Basarili"] = "Cihaz bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArizaIsaretle(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync($"Demirbaslar/{id}/ArizaIsaretle", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Cihaz arızalı olarak işaretlendi.";
            }
            else
            {
                TempData["Hata"] = (await response.Content.ReadAsStringAsync()).Trim('"');
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArizaGider(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync($"Demirbaslar/{id}/ArizaGider", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Basarili"] = "Cihazın arızası giderildi, cihaz tekrar boşta.";
            }
            else
            {
                            TempData["Hata"] = (await response.Content.ReadAsStringAsync()).Trim('"');
                            }

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }