using ITAssetManagement.Core;
using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class KullanicilarController : ControllerBase
    {
        private readonly UygulamaDbContext _context;

        public KullanicilarController(UygulamaDbContext context)
        {
            _context = context;
        }

        // 1. Tüm Kullanıcıları Listeleme
        [HttpGet]
        public async Task<IActionResult> GetirTumKullanicilar()
        {
            var kullanicilar = await _context.Kullanicilar.ToListAsync();
            return Ok(kullanicilar);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetKullanici(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null) return NotFound();

            return Ok(kullanici);
        }

        // 2. Yeni Kullanıcı Ekleme
        [HttpPost]
        public async Task<ActionResult<Kullanici>> PostKullanici(Kullanici kullanici)
        {
            // Eksik alanlar için güvenli varsayılanlar atıyoruz ki hata vermesin
            if (string.IsNullOrEmpty(kullanici.Rol))
            {
                kullanici.Rol = "Personel";
            }

            if (string.IsNullOrEmpty(kullanici.AdSoyad))
            {
                kullanici.AdSoyad = kullanici.KullaniciAdi;
            }

            // Şifreyi güvenli bir şekilde hashle
            if (!string.IsNullOrEmpty(kullanici.Sifre))
            {
                var hasher = new PasswordHasher<Kullanici>();
                kullanici.SifreHash = hasher.HashPassword(kullanici, kullanici.Sifre);
            }
            else
            {
                kullanici.SifreHash = string.Empty;
                kullanici.Sifre = "123456"; // Boş gelirse varsayılan şifre
            }

            kullanici.Onaylandi = true;

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetKullanici", new { id = kullanici.Id }, kullanici);
        }

        // Kullanıcı Güncelleme
        [HttpPut("{id}")]
        public async Task<IActionResult> KullaniciGuncelle(int id, Kullanici guncellenen)
        {
            if (id != guncellenen.Id) return BadRequest();

            var mevcut = await _context.Kullanicilar.FindAsync(id);
            if (mevcut == null) return NotFound();

            mevcut.KullaniciAdi = guncellenen.KullaniciAdi;
            mevcut.AdSoyad = guncellenen.AdSoyad;
            mevcut.Rol = guncellenen.Rol;

            if (!string.IsNullOrWhiteSpace(guncellenen.Sifre))
            {
                var hasher = new PasswordHasher<Kullanici>();
                mevcut.SifreHash = hasher.HashPassword(mevcut, guncellenen.Sifre);
            }

            _context.Entry(mevcut).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(mevcut);
        }

        // Toplu migration: düz metin şifreleri hash'leyip kaydet (bir kere çalıştırılmalı)
        [HttpPost("MigratePlainPasswords")]
        public async Task<IActionResult> MigratePlainPasswords()
        {
            var kullanicilar = await _context.Kullanicilar.ToListAsync();
            var hasher = new PasswordHasher<Kullanici>();
            var changed = new List<Kullanici>();

            foreach (var k in kullanicilar)
            {
                if (string.IsNullOrEmpty(k.SifreHash)) continue;
                // Basit tespit: Identity PasswordHasher çıktıları genelde "AQAAAAEA" ile başlar
                if (!k.SifreHash.StartsWith("AQAAAAEA"))
                {
                    var plain = k.SifreHash;
                    k.SifreHash = hasher.HashPassword(k, plain);
                    changed.Add(k);
                    _context.Entry(k).State = EntityState.Modified;
                }
            }

            if (changed.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new { migrated = changed.Count, total = kullanicilar.Count });
        }
        [HttpPost("{id}/Onayla")]
        public async Task<IActionResult> KullaniciOnayla(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null) return NotFound();

            kullanici.Onaylandi = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> KullaniciSil(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            // ÖNEMLİ: Personelin üzerinde zimmet var mı diye kontrol ediyoruz!
            var uzerindekiZimmetler = _context.Zimmetler.Any(z => z.KullaniciId == id);
            if (uzerindekiZimmetler)
            {
                // Üzerinde cihaz varsa hata fırlat ve silmeyi durdur
                return BadRequest("Bu personelin üzerinde zimmetli cihazlar var. Lütfen önce cihazları iade alın!");
            }

            _context.Kullanicilar.Remove(kullanici);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}