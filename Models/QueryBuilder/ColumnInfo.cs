namespace NwConsultas.Models.QueryBuilder
{
    /// <summary>
    /// Informações sobre uma coluna de tabela
    /// </summary>
    public class ColumnInfo
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? ColumnAlias { get; set; }
        public bool IsSelected { get; set; }
    }
}
