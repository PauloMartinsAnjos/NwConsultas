namespace NwConsultas.Models.QueryBuilder
{
    /// <summary>
    /// Operadores de filtro suportados
    /// </summary>
    public enum FilterOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Like,
        In,
        Between
    }

    /// <summary>
    /// Operador lógico para combinar filtros
    /// </summary>
    public enum LogicalOperator
    {
        And,
        Or
    }

    /// <summary>
    /// Condição de filtro (WHERE)
    /// </summary>
    public class FilterCondition
    {
        public string Column { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public FilterOperator Operator { get; set; } = FilterOperator.Equals;
        public string Value { get; set; } = string.Empty;
        public string? Value2 { get; set; } // Para BETWEEN
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
    }
}
