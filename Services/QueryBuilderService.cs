using NwConsultas.Models.QueryBuilder;
using System.Text;

namespace NwConsultas.Services
{
    /// <summary>
    /// Serviço responsável por construir SQL a partir de definições visuais
    /// </summary>
    public class QueryBuilderService : IQueryBuilderService
    {
        private readonly ILogger<QueryBuilderService> _logger;

        public QueryBuilderService(ILogger<QueryBuilderService> logger)
        {
            _logger = logger;
        }

        public string GenerateSql(QueryDefinition queryDefinition)
        {
            var sql = new StringBuilder();

            // SELECT
            sql.Append("SELECT ");
            
            if (queryDefinition.SelectedColumns.Count == 0)
            {
                sql.Append("*");
            }
            else
            {
                var columns = new List<string>();
                foreach (var col in queryDefinition.SelectedColumns)
                {
                    var columnRef = $"{col.TableName}.{col.ColumnName}";
                    
                    // Aplicar alias se existir
                    var alias = queryDefinition.Aliases
                        .FirstOrDefault(a => a.TableName == col.TableName && a.ColumnName == col.ColumnName);
                    
                    if (alias != null && !string.IsNullOrWhiteSpace(alias.Alias))
                    {
                        columnRef += $" AS \"{alias.Alias}\"";
                    }
                    
                    columns.Add(columnRef);
                }
                sql.Append(string.Join(", ", columns));
            }

            // FROM
            if (queryDefinition.Tables.Count > 0)
            {
                var mainTable = queryDefinition.Tables[0];
                sql.Append($"\nFROM {mainTable.TableName}");
                
                if (!string.IsNullOrWhiteSpace(mainTable.TableAlias))
                {
                    sql.Append($" AS {mainTable.TableAlias}");
                }
            }

            // JOINs
            foreach (var join in queryDefinition.Joins)
            {
                sql.Append("\n");
                
                switch (join.JoinType)
                {
                    case JoinType.Inner:
                        sql.Append("INNER JOIN");
                        break;
                    case JoinType.Left:
                        sql.Append("LEFT JOIN");
                        break;
                    case JoinType.Right:
                        sql.Append("RIGHT JOIN");
                        break;
                    case JoinType.Full:
                        sql.Append("FULL OUTER JOIN");
                        break;
                }

                sql.Append($" {join.RightTable}");
                sql.Append($" ON {join.LeftTable}.{join.LeftColumn} = {join.RightTable}.{join.RightColumn}");
            }

            // WHERE
            if (queryDefinition.Filters.Count > 0)
            {
                sql.Append("\nWHERE ");
                
                for (int i = 0; i < queryDefinition.Filters.Count; i++)
                {
                    var filter = queryDefinition.Filters[i];
                    
                    if (i > 0)
                    {
                        sql.Append(filter.LogicalOperator == LogicalOperator.And ? " AND " : " OR ");
                    }

                    var columnRef = $"{filter.TableName}.{filter.Column}";
                    
                    switch (filter.Operator)
                    {
                        case FilterOperator.Equals:
                            sql.Append($"{columnRef} = '{EscapeSqlValue(filter.Value)}'");
                            break;
                        case FilterOperator.NotEquals:
                            sql.Append($"{columnRef} != '{EscapeSqlValue(filter.Value)}'");
                            break;
                        case FilterOperator.GreaterThan:
                            sql.Append($"{columnRef} > '{EscapeSqlValue(filter.Value)}'");
                            break;
                        case FilterOperator.LessThan:
                            sql.Append($"{columnRef} < '{EscapeSqlValue(filter.Value)}'");
                            break;
                        case FilterOperator.GreaterThanOrEqual:
                            sql.Append($"{columnRef} >= '{EscapeSqlValue(filter.Value)}'");
                            break;
                        case FilterOperator.LessThanOrEqual:
                            sql.Append($"{columnRef} <= '{EscapeSqlValue(filter.Value)}'");
                            break;
                        case FilterOperator.Like:
                            sql.Append($"{columnRef} LIKE '%{EscapeSqlValue(filter.Value)}%'");
                            break;
                        case FilterOperator.In:
                            var inValues = filter.Value.Split(',').Select(v => $"'{EscapeSqlValue(v.Trim())}'");
                            sql.Append($"{columnRef} IN ({string.Join(", ", inValues)})");
                            break;
                        case FilterOperator.Between:
                            sql.Append($"{columnRef} BETWEEN '{EscapeSqlValue(filter.Value)}' AND '{EscapeSqlValue(filter.Value2 ?? "")}'");
                            break;
                    }
                }
            }

            var generatedSql = sql.ToString();
            _logger.LogInformation($"SQL gerado: {generatedSql}");
            
            return generatedSql;
        }

        public (bool IsValid, List<string> Errors) ValidateQuery(QueryDefinition queryDefinition)
        {
            var errors = new List<string>();

            // Validar se há pelo menos uma tabela
            if (queryDefinition.Tables.Count == 0)
            {
                errors.Add("Pelo menos uma tabela deve ser selecionada");
            }

            // Validar se há colunas selecionadas
            if (queryDefinition.SelectedColumns.Count == 0)
            {
                errors.Add("Pelo menos uma coluna deve ser selecionada");
            }

            // Validar JOINs
            foreach (var join in queryDefinition.Joins)
            {
                if (string.IsNullOrWhiteSpace(join.LeftTable) || 
                    string.IsNullOrWhiteSpace(join.RightTable))
                {
                    errors.Add("JOIN inválido: tabelas não especificadas");
                }

                if (string.IsNullOrWhiteSpace(join.LeftColumn) || 
                    string.IsNullOrWhiteSpace(join.RightColumn))
                {
                    errors.Add("JOIN inválido: colunas não especificadas");
                }
            }

            // Validar filtros
            foreach (var filter in queryDefinition.Filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Column))
                {
                    errors.Add("Filtro inválido: coluna não especificada");
                }

                if (filter.Operator == FilterOperator.Between && 
                    string.IsNullOrWhiteSpace(filter.Value2))
                {
                    errors.Add("Filtro BETWEEN requer dois valores");
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Escapa valores para prevenir SQL injection
        /// </summary>
        private string EscapeSqlValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Replace single quotes with two single quotes (SQL standard escaping)
            return value.Replace("'", "''");
        }
    }
}
