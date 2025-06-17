using Microsoft.EntityFrameworkCore;
using EnrollmentService.Models;

namespace EnrollmentService.Data
{
    /// <summary>
    /// Database context for the Enrollment Service
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        /// <summary>
        /// Enrollments table
        /// </summary>
        public DbSet<Enrollment> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da tabela matriculas
            modelBuilder.Entity<Enrollment>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);
                
                // Required fields (campos originais)
                entity.Property(e => e.IdAluno).IsRequired();
                entity.Property(e => e.IdCurso).IsRequired();
                entity.Property(e => e.DataMatricula).IsRequired();

                // Novos campos adicionados
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Enrolled");

                entity.Property(e => e.Notes)
                    .HasMaxLength(500);

                // Índice único para evitar matrículas duplicadas (mantido do original)
                entity.HasIndex(e => new { e.IdAluno, e.IdCurso })
                    .IsUnique()
                    .HasDatabaseName("IX_Enrollment_Student_Course");

                // Índices adicionais para performance
                entity.HasIndex(e => e.IdAluno)
                    .HasDatabaseName("IX_Enrollment_Student");

                entity.HasIndex(e => e.IdCurso)
                    .HasDatabaseName("IX_Enrollment_Course");

                entity.HasIndex(e => e.DataMatricula)
                    .HasDatabaseName("IX_Enrollment_Date");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Enrollment_Status");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_Enrollment_IsActive");

                // Índice composto para consultas de matrículas ativas
                entity.HasIndex(e => new { e.IsActive, e.Status })
                    .HasDatabaseName("IX_Enrollment_Active_Status");
            });

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Override SaveChanges to automatically update timestamps
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update timestamps
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates timestamps for entities being modified
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<Enrollment>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // Previne que CreatedAt seja modificado
                    entry.Property(e => e.CreatedAt).IsModified = false;
                }
            }
        }
    }
}