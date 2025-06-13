using Microsoft.EntityFrameworkCore;
using EnrollmentService.Models;

namespace EnrollmentService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Enrollment> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura��o da tabela matriculas
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IdAluno).IsRequired();
                entity.Property(e => e.IdCurso).IsRequired();
                entity.Property(e => e.DataMatricula).IsRequired();

                // �ndice �nico para evitar matr�culas duplicadas
                entity.HasIndex(e => new { e.IdAluno, e.IdCurso }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}