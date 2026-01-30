using System.Data;

namespace NwConsultas.Services
{
    /// <summary>
    /// Interface para serviço de exportação de resultados
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exporta DataTable para CSV
        /// </summary>
        byte[] ExportToCsv(DataTable data, string? queryName = null);

        /// <summary>
        /// Exporta DataTable para Excel
        /// </summary>
        byte[] ExportToExcel(DataTable data, string? queryName = null);

        /// <summary>
        /// Exporta DataTable para JSON
        /// </summary>
        string ExportToJson(DataTable data);
    }
}
