using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NwConsultas.Database;
using NwConsultas.Models.Database;
using NwConsultas.Services;
using System.Data;
using System.Text;

namespace NwConsultas.Controllers
{
    /// <summary>
    /// Controller para exportação de resultados
    /// </summary>
    public class ExportController : Controller
    {
        private readonly IExportService _exportService;
        private readonly NwConsultasDbContext _context;
        private readonly ILogger<ExportController> _logger;

        public ExportController(
            IExportService exportService,
            NwConsultasDbContext context,
            ILogger<ExportController> logger)
        {
            _exportService = exportService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Exporta resultados para CSV
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Csv([FromBody] ExportRequest request)
        {
            try
            {
                var dataTable = JsonToDataTable(request.Data);
                var csvBytes = _exportService.ExportToCsv(dataTable, request.QueryName);

                // Registrar exportação
                await RegisterExport(request.SavedQueryId, "CSV", $"{request.QueryName ?? "query"}.csv", dataTable.Rows.Count);

                var fileName = $"{request.QueryName ?? "resultados"}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar para CSV");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Exporta resultados para Excel
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Excel([FromBody] ExportRequest request)
        {
            try
            {
                var dataTable = JsonToDataTable(request.Data);
                var excelBytes = _exportService.ExportToExcel(dataTable, request.QueryName);

                // Registrar exportação
                await RegisterExport(request.SavedQueryId, "XLSX", $"{request.QueryName ?? "query"}.xlsx", dataTable.Rows.Count);

                var fileName = $"{request.QueryName ?? "resultados"}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar para Excel");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Exporta resultados para JSON
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Json([FromBody] ExportRequest request)
        {
            try
            {
                var dataTable = JsonToDataTable(request.Data);
                var json = _exportService.ExportToJson(dataTable);

                // Registrar exportação
                await RegisterExport(request.SavedQueryId, "JSON", $"{request.QueryName ?? "query"}.json", dataTable.Rows.Count);

                var fileName = $"{request.QueryName ?? "resultados"}_{DateTime.Now:yyyyMMddHHmmss}.json";
                var bytes = Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar para JSON");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Converte JSON array para DataTable
        /// </summary>
        private DataTable JsonToDataTable(string json)
        {
            var dataTable = new DataTable();
            var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            if (data == null || data.Count == 0)
                return dataTable;

            // Criar colunas
            foreach (var key in data[0].Keys)
            {
                dataTable.Columns.Add(key);
            }

            // Adicionar linhas
            foreach (var row in data)
            {
                var dataRow = dataTable.NewRow();
                foreach (var key in row.Keys)
                {
                    dataRow[key] = row[key]?.ToString() ?? "";
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Registra exportação no banco
        /// </summary>
        private async Task RegisterExport(int? savedQueryId, string format, string fileName, int rowCount)
        {
            var export = new QueryExport
            {
                SavedQueryId = savedQueryId,
                ExportFormat = format,
                FileName = fileName,
                RowCount = rowCount,
                ExportedBy = "system"
            };

            _context.QueryExports.Add(export);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Request para exportação
    /// </summary>
    public class ExportRequest
    {
        public string Data { get; set; } = string.Empty;
        public string? QueryName { get; set; }
        public int? SavedQueryId { get; set; }
    }
}
