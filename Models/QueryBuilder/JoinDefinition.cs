namespace NwConsultas.Models.QueryBuilder
{
    /// <summary>
    /// Tipos de JOIN suportados
    /// </summary>
    public enum JoinType
    {
        Inner,
        Left,
        Right,
        Full
    }

    /// <summary>
    /// Definição de um JOIN entre tabelas
    /// </summary>
    public class JoinDefinition
    {
        public string LeftTable { get; set; } = string.Empty;
        public string LeftColumn { get; set; } = string.Empty;
        public string RightTable { get; set; } = string.Empty;
        public string RightColumn { get; set; } = string.Empty;
        public JoinType JoinType { get; set; } = JoinType.Inner;
    }
}
