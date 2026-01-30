using NwConsultas.Models.QueryBuilder;
using System.Data;

namespace NwConsultas.Services
{
    /// <summary>
    /// Interface para serviço de conexão com Firebird
    /// </summary>
    public interface IFirebirdService
    {
        /// <summary>
        /// Obtém lista de todas as tabelas do banco
        /// </summary>
        Task<List<TableInfo>> GetTablesAsync();

        /// <summary>
        /// Obtém colunas de uma tabela específica
        /// </summary>
        Task<List<ColumnInfo>> GetColumnsAsync(string tableName);

        /// <summary>
        /// Executa uma query SQL e retorna os resultados
        /// </summary>
        Task<DataTable> ExecuteQueryAsync(string sql, int maxRows = 10000);

        /// <summary>
        /// Testa a conexão com o banco
        /// </summary>
        Task<bool> TestConnectionAsync();
    }
}
