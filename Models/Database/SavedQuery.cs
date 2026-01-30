using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NwConsultas.Models.Database
{
    /// <summary>
    /// Representa uma query salva no sistema
    /// </summary>
    [Table("saved_queries")]
    public class SavedQuery
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Estrutura JSON da query (tabelas, joins, filtros, etc)
        /// </summary>
        [Required]
        [Column("query_json")]
        public string QueryJson { get; set; } = string.Empty;

        /// <summary>
        /// SQL gerado a partir da estrutura visual
        /// </summary>
        [Required]
        [Column("sql_generated")]
        public string SqlGenerated { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "system";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navegação
        public virtual ICollection<QueryExecution> Executions { get; set; } = new List<QueryExecution>();
        public virtual ICollection<QueryExport> Exports { get; set; } = new List<QueryExport>();
    }
}
