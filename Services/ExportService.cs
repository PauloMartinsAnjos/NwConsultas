using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Text;

namespace NwConsultas.Services
{
    /// <summary>
    /// Serviço para exportação de resultados em diferentes formatos
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
        }

        public byte[] ExportToCsv(DataTable data, string? queryName = null)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";"
                };
                
                using var csvWriter = new CsvWriter(streamWriter, config);

                // Escrever cabeçalhos
                foreach (DataColumn column in data.Columns)
                {
                    csvWriter.WriteField(column.ColumnName);
                }
                csvWriter.NextRecord();

                // Escrever dados
                foreach (DataRow row in data.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        csvWriter.WriteField(item?.ToString() ?? "");
                    }
                    csvWriter.NextRecord();
                }

                streamWriter.Flush();
                var result = memoryStream.ToArray();
                
                _logger.LogInformation($"Exportação CSV concluída. {data.Rows.Count} linhas exportadas");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar para CSV");
                throw;
            }
        }

        public byte[] ExportToExcel(DataTable data, string? queryName = null)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(queryName ?? "Resultados");

                // Adicionar cabeçalhos
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = data.Columns[i].ColumnName;
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Adicionar dados
                for (int row = 0; row < data.Rows.Count; row++)
                {
                    for (int col = 0; col < data.Columns.Count; col++)
                    {
                        var value = data.Rows[row][col];
                        worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
                    }
                }

                // Auto-ajustar colunas
                worksheet.Columns().AdjustToContents();

                // Adicionar metadados
                var metadataRow = data.Rows.Count + 3;
                worksheet.Cell(metadataRow, 1).Value = "Exportado em:";
                worksheet.Cell(metadataRow, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                
                if (!string.IsNullOrWhiteSpace(queryName))
                {
                    worksheet.Cell(metadataRow + 1, 1).Value = "Query:";
                    worksheet.Cell(metadataRow + 1, 2).Value = queryName;
                }

                using var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                var result = memoryStream.ToArray();
                
                _logger.LogInformation($"Exportação Excel concluída. {data.Rows.Count} linhas exportadas");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar para Excel");
                throw;
            }
        }

        public string ExportToJson(DataTable data)
        {
            try
            {
                var rows = new List<Dictionary<string, object?>>();

                foreach (DataRow row in data.Rows)
                {
                    var dict = new Dictionary<string, object?>();
                    
                    foreach (DataColumn column in data.Columns)
                    {
                        dict[column.ColumnName] = row[column];
                    }
                    
                    rows.Add(dict);
                }

                var json = JsonConvert.SerializeObject(new
                {
                    ExportedAt = DateTime.Now,
                    RowCount = data.Rows.Count,
                    Data = rows
                }, Formatting.Indented);

                _logger.LogInformation($"Exportação JSON concluída. {data.Rows.Count} linhas exportadas");
                return json;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar para JSON");
                throw;
            }
        }
    }
}
