namespace NwConsultas.Models.QueryBuilder
{
    /// <summary>
    /// Definição completa de uma query construída visualmente
    /// </summary>
    public class QueryDefinition
    {
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        public List<JoinDefinition> Joins { get; set; } = new List<JoinDefinition>();
        public List<ColumnInfo> SelectedColumns { get; set; } = new List<ColumnInfo>();
        public List<FilterCondition> Filters { get; set; } = new List<FilterCondition>();
        public List<ColumnAlias> Aliases { get; set; } = new List<ColumnAlias>();
        public int? MaxRows { get; set; }
    }
}
