using Microsoft.EntityFrameworkCore;
using NwConsultas.Models.Database;

namespace NwConsultas.Database
{
    /// <summary>
    /// DbContext para o banco PostgreSQL que armazena queries e histórico
    /// </summary>
    public class NwConsultasDbContext : DbContext
    {
        public NwConsultasDbContext(DbContextOptions<NwConsultasDbContext> options)
            : base(options)
        {
        }

        public DbSet<SavedQuery> SavedQueries { get; set; }
        public DbSet<QueryExecution> QueryExecutions { get; set; }
        public DbSet<QueryExport> QueryExports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações adicionais se necessário
            modelBuilder.Entity<SavedQuery>(entity =>
            {
                entity.HasMany(q => q.Executions)
                    .WithOne(e => e.SavedQuery)
                    .HasForeignKey(e => e.SavedQueryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(q => q.Exports)
                    .WithOne(e => e.SavedQuery)
                    .HasForeignKey(e => e.SavedQueryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
