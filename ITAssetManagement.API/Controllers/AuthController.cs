using ITAssetManagement.Core;
using ITAssetManagement.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UygulamaDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(UygulamaDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public class GirisIstegi
        {
            public string KullaniciAdi { get; set; } = string.Empty;
            public string Sifre { get; set; } = string.Empty;
        }

        [HttpPost("Token")]
        public async Task<IActionResult> Token([FromBody] GirisIstegi istek)
        {
            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi != null && k.KullaniciAdi.ToLower() == istek.KullaniciAdi.Trim().ToLower());

            if (kullanici == null || string.IsNullOrEmpty(kullanici.SifreHash))
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            var hasher = new PasswordHasher<Kullanici>();
            if (hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, istek.Sifre) != PasswordVerificationResult.Success)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");
            }

            if (!kullanici.Onaylandi && kullanici.Rol != "Admin")
            {
                return Unauthorized("Hesabınız henüz yönetici tarafından onaylanmadı.");
            }

            var rol = string.IsNullOrEmpty(kullanici.Rol) ? "Personel" : kullanici.Rol;
            var jwtAyarlari = _configuration.GetSection("Jwt");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, kullanici.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, kullanici.AdSoyad ?? kullanici.KullaniciAdi),
                new Claim(ClaimTypes.Role, rol)
            };

            var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAyarlari["Key"]!));
            var kimlik = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256);
            var sureDakika = jwtAyarlari.GetValue<int>("ExpireMinutes", 120);

            var token = new JwtSecurityToken(
                issuer: jwtAyarlari["Issuer"],
                audience: jwtAyarlari["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(sureDakika),
                signingCredentials: kimlik);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                rol,
                kullaniciAdi = kullanici.KullaniciAdi,
                adSoyad = kullanici.AdSoyad
            });
        }
    }
}
