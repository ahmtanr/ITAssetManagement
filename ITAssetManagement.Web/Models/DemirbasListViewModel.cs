using ITAssetManagement.Core;

namespace ITAssetManagement.Web.Models
{
    public class DemirbasListViewModel
    {
        public List<Demirbas> Items { get; set; } = new List<Demirbas>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Query { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
    }
}
