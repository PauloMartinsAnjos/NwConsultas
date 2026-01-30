using System.Data;

namespace NwConsultas.Models.ViewModels
{
    /// <summary>
    /// ViewModel para exibição de resultados de query
    /// </summary>
    public class QueryResultViewModel
    {
        public DataTable? Results { get; set; }
        public string? GeneratedSql { get; set; }
        public int RowCount { get; set; }
        public long ExecutionTimeMs { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? SavedQueryId { get; set; }
        public string? QueryName { get; set; }
    }
}
