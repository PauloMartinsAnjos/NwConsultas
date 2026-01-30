using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NwConsultas.Models.Database
{
    /// <summary>
    /// Representa uma execução de query (histórico)
    /// </summary>
    [Table("query_executions")]
    public class QueryExecution
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("saved_query_id")]
        public int? SavedQueryId { get; set; }

        [Column("executed_at")]
        public DateTime ExecutedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Tempo de execução em milissegundos
        /// </summary>
        [Column("execution_time_ms")]
        public int? ExecutionTimeMs { get; set; }

        [Column("rows_returned")]
        public int? RowsReturned { get; set; }

        [MaxLength(100)]
        [Column("executed_by")]
        public string ExecutedBy { get; set; } = "system";

        [Column("success")]
        public bool Success { get; set; } = true;

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        // Navegação
        [ForeignKey("SavedQueryId")]
        public virtual SavedQuery? SavedQuery { get; set; }
    }
}
