using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NwConsultas.Models.Database
{
    /// <summary>
    /// Representa uma exportação de resultados
    /// </summary>
    [Table("query_exports")]
    public class QueryExport
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("saved_query_id")]
        public int? SavedQueryId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("export_format")]
        public string ExportFormat { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("file_name")]
        public string? FileName { get; set; }

        [Column("exported_at")]
        public DateTime ExportedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        [Column("exported_by")]
        public string ExportedBy { get; set; } = "system";

        [Column("row_count")]
        public int? RowCount { get; set; }

        // Navegação
        [ForeignKey("SavedQueryId")]
        public virtual SavedQuery? SavedQuery { get; set; }
    }
}
