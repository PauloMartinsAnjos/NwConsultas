using NwConsultas.Models.QueryBuilder;

namespace NwConsultas.Models.ViewModels
{
    /// <summary>
    /// ViewModel para a página principal do Query Builder
    /// </summary>
    public class QueryBuilderViewModel
    {
        public List<TableInfo> AvailableTables { get; set; } = new List<TableInfo>();
        public QueryDefinition CurrentQuery { get; set; } = new QueryDefinition();
        public string? GeneratedSql { get; set; }
        public int? SavedQueryId { get; set; }
        public string? QueryName { get; set; }
        public string? QueryDescription { get; set; }
    }
}
