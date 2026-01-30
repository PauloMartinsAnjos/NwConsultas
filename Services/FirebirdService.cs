using FirebirdSql.Data.FirebirdClient;
using NwConsultas.Models.QueryBuilder;
using System.Data;

namespace NwConsultas.Services
{
    /// <summary>
    /// Serviço para conexão e operações com Firebird 2.5
    /// </summary>
    public class FirebirdService : IFirebirdService
    {
        private readonly string _connectionString;
        private readonly ILogger<FirebirdService> _logger;

        public FirebirdService(IConfiguration configuration, ILogger<FirebirdService> logger)
        {
            _connectionString = configuration.GetConnectionString("Firebird") 
                ?? throw new InvalidOperationException("Firebird connection string não configurada");
            _logger = logger;
        }

        public async Task<List<TableInfo>> GetTablesAsync()
        {
            var tables = new List<TableInfo>();

            try
            {
                using var connection = new FbConnection(_connectionString);
                await connection.OpenAsync();

                // Query para obter tabelas do sistema Firebird
                var sql = @"
                    SELECT RDB$RELATION_NAME 
                    FROM RDB$RELATIONS 
                    WHERE RDB$SYSTEM_FLAG = 0 
                    AND RDB$VIEW_BLR IS NULL
                    ORDER BY RDB$RELATION_NAME";

                using var command = new FbCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString(0).Trim();
                    tables.Add(new TableInfo { TableName = tableName });
                }

                _logger.LogInformation($"Carregadas {tables.Count} tabelas do Firebird");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tabelas do Firebird");
                throw;
            }

            return tables;
        }

        public async Task<List<ColumnInfo>> GetColumnsAsync(string tableName)
        {
            var columns = new List<ColumnInfo>();

            try
            {
                _logger.LogInformation($"🔄 Buscando colunas do Firebird para tabela: {tableName}");
                
                using var connection = new FbConnection(_connectionString);
                await connection.OpenAsync();

                // Query para obter colunas de uma tabela
                var sql = @"
                    SELECT 
                        RF.RDB$FIELD_NAME AS FIELD_NAME,
                        F.RDB$FIELD_TYPE AS FIELD_TYPE,
                        F.RDB$FIELD_LENGTH AS FIELD_LENGTH,
                        RF.RDB$NULL_FLAG AS NULL_FLAG
                    FROM RDB$RELATION_FIELDS RF
                    JOIN RDB$FIELDS F ON RF.RDB$FIELD_SOURCE = F.RDB$FIELD_NAME
                    WHERE RF.RDB$RELATION_NAME = @TableName
                    ORDER BY RF.RDB$FIELD_POSITION";

                using var command = new FbCommand(sql, connection);
                command.Parameters.AddWithValue("@TableName", tableName.ToUpper());

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(0).Trim();
                    
                    // Validar se nome da coluna não está vazio
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        _logger.LogWarning($"⚠️ Coluna com nome vazio detectada na tabela {tableName}");
                        continue;
                    }
                    
                    var fieldType = reader.GetInt16(1);
                    var isNullable = reader.IsDBNull(3) ? true : reader.GetInt16(3) == 0;

                    columns.Add(new ColumnInfo
                    {
                        ColumnName = columnName,
                        TableName = tableName,
                        DataType = MapFirebirdType(fieldType),
                        IsNullable = isNullable
                    });
                }

                _logger.LogInformation($"✅ Firebird retornou {columns.Count} colunas para tabela {tableName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erro ao buscar colunas do Firebird para tabela {tableName}");
                throw;
            }

            return columns;
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, int maxRows = 10000)
        {
            var dataTable = new DataTable();

            try
            {
                using var connection = new FbConnection(_connectionString);
                await connection.OpenAsync();

                // Limitar resultados
                var limitedSql = $"SELECT FIRST {maxRows} * FROM ({sql}) AS limited_query";

                using var command = new FbCommand(limitedSql, connection);
                command.CommandTimeout = 300; // 5 minutos

                using var adapter = new FbDataAdapter(command);
                adapter.Fill(dataTable);

                _logger.LogInformation($"Query executada com sucesso. {dataTable.Rows.Count} linhas retornadas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar query no Firebird");
                throw;
            }

            return dataTable;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new FbConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Conexão com Firebird testada com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar conexão com Firebird");
                return false;
            }
        }

        /// <summary>
        /// Mapeia tipos do Firebird para tipos mais legíveis
        /// </summary>
        private string MapFirebirdType(short fieldType)
        {
            return fieldType switch
            {
                7 => "SMALLINT",
                8 => "INTEGER",
                10 => "FLOAT",
                12 => "DATE",
                13 => "TIME",
                14 => "CHAR",
                16 => "BIGINT",
                27 => "DOUBLE",
                35 => "TIMESTAMP",
                37 => "VARCHAR",
                261 => "BLOB",
                _ => "UNKNOWN"
            };
        }
    }
}
