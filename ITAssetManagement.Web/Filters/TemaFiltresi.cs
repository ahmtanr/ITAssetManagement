using ITAssetManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ITAssetManagement.Web.Filters
{
    public class TemaFiltresi : IAsyncResultFilter
    {
        private readonly UygulamaDbContext _context;

        public TemaFiltresi(UygulamaDbContext context)
        {
            _context = context;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Controller is Controller controller)
            {
                var ayar = await _context.Ayarlar.AsNoTracking().FirstOrDefaultAsync();
                controller.ViewData["Tema"] = ayar?.Tema ?? "Acik";
            }

            await next();
        }
    }
}
