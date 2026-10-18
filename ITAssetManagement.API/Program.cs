using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (SQL Server)
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1.1 JWT Kimlik Doğrulama
var jwtAyarlari = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtAyarlari["Issuer"],
            ValidAudience = jwtAyarlari["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAyarlari["Key"]!)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// 2. Controller ve JSON Döngü Koruması
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 3. Swagger Servisleri
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var builderApp = builder.Build();

// 4. Otomatik Migration ve Otomatik Admin (Seed) Ekleme
using (var scope = builderApp.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UygulamaDbContext>();
    db.Database.Migrate();

    // Veritabanında hiç Admin rolüne sahip kullanıcı var mı kontrol ediyoruz
    bool adminVarMi = db.Kullanicilar.Any(k => k.Rol == "Admin");

    if (!adminVarMi)
    {
        var adminKullanici = new Kullanici
        {
            KullaniciAdi = "admin",
            Sifre = "123456",
            Rol = "Admin",
            AdSoyad = "Sistem Yoneticisi",
            Onaylandi = true
        };

        db.Kullanicilar.Add(adminKullanici);
        db.SaveChanges();
    }
}

// 5. Middleware Ayarları
if (builderApp.Environment.IsDevelopment() ||
    builderApp.Environment.IsEnvironment("Docker"))
{
    builderApp.UseSwagger();
    builderApp.UseSwaggerUI();
}

if (!builderApp.Environment.IsEnvironment("Docker"))
{
    builderApp.UseHttpsRedirection();
}

builderApp.UseAuthentication();
builderApp.UseAuthorization();

builderApp.MapControllers();

builderApp.Run();
