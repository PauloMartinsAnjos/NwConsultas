namespace NwConsultas.Models.QueryBuilder
{
    /// <summary>
    /// Informações sobre uma tabela do Firebird
    /// </summary>
    public class TableInfo
    {
        public string TableName { get; set; } = string.Empty;
        public string? TableAlias { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }
}
