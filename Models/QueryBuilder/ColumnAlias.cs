namespace NwConsultas.Models.QueryBuilder
{
    /// <summary>
    /// Alias de coluna para personalização do nome exibido
    /// </summary>
    public class ColumnAlias
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }
}
