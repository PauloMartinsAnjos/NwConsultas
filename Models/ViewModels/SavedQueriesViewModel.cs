using NwConsultas.Models.Database;

namespace NwConsultas.Models.ViewModels
{
    /// <summary>
    /// ViewModel para listagem de queries salvas
    /// </summary>
    public class SavedQueriesViewModel
    {
        public List<SavedQuery> Queries { get; set; } = new List<SavedQuery>();
        public string? SearchTerm { get; set; }
        public int TotalQueries { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }
}
