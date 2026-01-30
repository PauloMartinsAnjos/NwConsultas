using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NwConsultas.Database;
using NwConsultas.Models.Database;
using NwConsultas.Models.QueryBuilder;
using NwConsultas.Models.ViewModels;
using NwConsultas.Services;
using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;

namespace NwConsultas.Controllers
{
    /// <summary>
    /// Controller para construção visual de queries
    /// </summary>
    public class QueryBuilderController : Controller
    {
        private readonly IFirebirdService _firebirdService;
        private readonly IQueryBuilderService _queryBuilderService;
        private readonly NwConsultasDbContext _context;
        private readonly ILogger<QueryBuilderController> _logger;
        private readonly IConfiguration _configuration;

        public QueryBuilderController(
            IFirebirdService firebirdService,
            IQueryBuilderService queryBuilderService,
            NwConsultasDbContext context,
            ILogger<QueryBuilderController> logger,
            IConfiguration configuration)
        {
            _firebirdService = firebirdService;
            _queryBuilderService = queryBuilderService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Página principal do Query Builder
        /// </summary>
        public async Task<IActionResult> Index(int? id)
        {
            var viewModel = new QueryBuilderViewModel();

            try
            {
                // Carregar tabelas disponíveis do Firebird
                viewModel.AvailableTables = await _firebirdService.GetTablesAsync();

                // Se ID fornecido, carregar query salva
                if (id.HasValue)
                {
                    var savedQuery = await _context.SavedQueries.FindAsync(id.Value);
                    if (savedQuery != null)
                    {
                        viewModel.SavedQueryId = savedQuery.Id;
                        viewModel.QueryName = savedQuery.Name;
                        viewModel.QueryDescription = savedQuery.Description;
                        viewModel.CurrentQuery = JsonConvert.DeserializeObject<QueryDefinition>(savedQuery.QueryJson) 
                            ?? new QueryDefinition();
                        viewModel.GeneratedSql = savedQuery.SqlGenerated;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar Query Builder");
                ViewBag.ErrorMessage = "Erro ao conectar com o banco de dados Firebird. Verifique a configuração.";
            }

            return View(viewModel);
        }

        /// <summary>
        /// Carrega colunas de uma tabela
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTableColumns(string tableName)
        {
            try
            {
                _logger.LogInformation($"🔄 Carregando colunas da tabela: {tableName}");
                
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    _logger.LogWarning("❌ Nome da tabela não fornecido");
                    return BadRequest(new { error = "Nome da tabela é obrigatório" });
                }
                
                var columns = await _firebirdService.GetColumnsAsync(tableName);
                
                if (columns == null)
                {
                    _logger.LogWarning($"⚠️ GetColumnsAsync retornou null para tabela: {tableName}");
                    return Ok(new List<object>()); // Retornar array vazio ao invés de null
                }
                
                _logger.LogInformation($"✅ {columns.Count} colunas carregadas da tabela {tableName}");
                
                return Json(columns);
            }
            catch (FbException fbEx)
            {
                _logger.LogError(fbEx, $"❌ Erro Firebird ao carregar colunas da tabela {tableName}");
                return StatusCode(500, new { error = $"Erro no banco Firebird: {fbEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erro ao carregar colunas da tabela {tableName}");
                return StatusCode(500, new { error = $"Erro ao carregar colunas: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gera SQL a partir da definição visual
        /// </summary>
        [HttpPost]
        public IActionResult GenerateSql([FromBody] QueryDefinition queryDefinition)
        {
            try
            {
                // Validar query
                var (isValid, errors) = _queryBuilderService.ValidateQuery(queryDefinition);
                if (!isValid)
                {
                    return BadRequest(new { errors });
                }

                // Gerar SQL
                var sql = _queryBuilderService.GenerateSql(queryDefinition);
                return Json(new { sql });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar SQL");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Executa a query
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] ExecuteQueryRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var viewModel = new QueryResultViewModel
            {
                GeneratedSql = request.Sql,
                SavedQueryId = request.SavedQueryId,
                QueryName = request.QueryName
            };

            try
            {
                // Obter limite de linhas da configuração
                var maxRows = _configuration.GetValue<int>("QueryBuilder:MaxResultRows", 10000);

                // Executar query
                viewModel.Results = await _firebirdService.ExecuteQueryAsync(request.Sql, maxRows);
                viewModel.RowCount = viewModel.Results.Rows.Count;
                viewModel.Success = true;

                stopwatch.Stop();
                viewModel.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                // Registrar execução no histórico
                var execution = new QueryExecution
                {
                    SavedQueryId = request.SavedQueryId,
                    ExecutionTimeMs = (int)viewModel.ExecutionTimeMs,
                    RowsReturned = viewModel.RowCount,
                    Success = true,
                    ExecutedBy = "system"
                };

                _context.QueryExecutions.Add(execution);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Query executada com sucesso. {viewModel.RowCount} linhas em {viewModel.ExecutionTimeMs}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                viewModel.Success = false;
                viewModel.ErrorMessage = ex.Message;
                viewModel.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                // Registrar erro no histórico
                var execution = new QueryExecution
                {
                    SavedQueryId = request.SavedQueryId,
                    ExecutionTimeMs = (int)viewModel.ExecutionTimeMs,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutedBy = "system"
                };

                _context.QueryExecutions.Add(execution);
                await _context.SaveChangesAsync();

                _logger.LogError(ex, "Erro ao executar query");
            }

            return View(viewModel);
        }

        /// <summary>
        /// Salva a query no PostgreSQL
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveQueryRequest request)
        {
            try
            {
                var queryJson = JsonConvert.SerializeObject(request.QueryDefinition);

                if (request.QueryId.HasValue)
                {
                    // Atualizar query existente
                    var existingQuery = await _context.SavedQueries.FindAsync(request.QueryId.Value);
                    if (existingQuery == null)
                    {
                        return NotFound(new { error = "Query não encontrada" });
                    }

                    existingQuery.Name = request.Name;
                    existingQuery.Description = request.Description;
                    existingQuery.QueryJson = queryJson;
                    existingQuery.SqlGenerated = request.GeneratedSql;
                    existingQuery.UpdatedAt = DateTime.Now;

                    _context.SavedQueries.Update(existingQuery);
                }
                else
                {
                    // Criar nova query
                    var newQuery = new SavedQuery
                    {
                        Name = request.Name,
                        Description = request.Description,
                        QueryJson = queryJson,
                        SqlGenerated = request.GeneratedSql,
                        CreatedBy = "system"
                    };

                    _context.SavedQueries.Add(newQuery);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Query salva com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar query");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request para executar query
    /// </summary>
    public class ExecuteQueryRequest
    {
        public string Sql { get; set; } = string.Empty;
        public int? SavedQueryId { get; set; }
        public string? QueryName { get; set; }
    }

    /// <summary>
    /// Request para salvar query
    /// </summary>
    public class SaveQueryRequest
    {
        public int? QueryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public QueryDefinition QueryDefinition { get; set; } = new QueryDefinition();
        public string GeneratedSql { get; set; } = string.Empty;
    }
}
