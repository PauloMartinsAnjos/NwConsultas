using NwConsultas.Models.QueryBuilder;

namespace NwConsultas.Services
{
    /// <summary>
    /// Interface para serviço de construção de queries
    /// </summary>
    public interface IQueryBuilderService
    {
        /// <summary>
        /// Gera SQL a partir de uma definição de query visual
        /// </summary>
        string GenerateSql(QueryDefinition queryDefinition);

        /// <summary>
        /// Valida uma definição de query
        /// </summary>
        (bool IsValid, List<string> Errors) ValidateQuery(QueryDefinition queryDefinition);
    }
}
