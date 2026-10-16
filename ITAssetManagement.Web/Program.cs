using ITAssetManagement.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC Servisleri
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ITAssetManagement.Web.Filters.TemaFiltresi>();
});

// 2. Veritabanı Bağlantı Servisi (UygulamaDbContext ekleniyor)
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Cookie Tabanlı Kimlik Doğrulama (Giriş / Çıkış yönetimi için)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Giris/Index"; // Giriş yapılmadıysa yönlendirilecek sayfa
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// 4. API Haberleşmesi İçin HttpClient Servisi (JWT Bearer ile)
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ITAssetManagement.Web.Services.JwtBearerHandler>();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7040/api/");
})
.AddHttpMessageHandler<ITAssetManagement.Web.Services.JwtBearerHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// 5. Kimlik Doğrulama ve Yetkilendirme Sıralaması (Çok Önemli!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Giris}/{action=Index}/{id?}");

app.Run();