using EnrollmentService.Data;
using EnrollmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentService.Repositories
{
    public interface IEnrollmentRepository
    {
        Task<IEnumerable<Enrollment>> GetAllAsync();
        Task<IEnumerable<Enrollment>> GetAllActiveAsync();
        Task<Enrollment?> GetByIdAsync(int id);
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetActiveByStudentIdAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Enrollment>> GetActiveByCourseIdAsync(int courseId);
        Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId);
        Task<Enrollment?> GetActiveByStudentAndCourseAsync(int studentId, int courseId);
        Task<Enrollment> CreateAsync(Enrollment enrollment);
        Task<Enrollment?> UpdateAsync(Enrollment enrollment);
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id, string? reason = null);
        Task<bool> StudentExistsInCourseAsync(int studentId, int courseId);
        Task<bool> StudentActiveInCourseAsync(int studentId, int courseId);
        Task<int> GetCourseEnrollmentCountAsync(int courseId);
        Task<int> GetActiveCourseEnrollmentCountAsync(int courseId);
        Task<IEnumerable<Enrollment>> GetByStatusAsync(string status);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }

    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly DataContext _context;

        public EnrollmentRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Enrollment>> GetAllAsync()
        {
            return await _context.Matriculas.ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetAllActiveAsync()
        {
            return await _context.Matriculas
                .Where(m => m.IsActive)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            return await _context.Matriculas.FindAsync(id);
        }

        public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Matriculas
                .Where(m => m.IdAluno == studentId)
                .OrderByDescending(m => m.DataMatricula)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetActiveByStudentIdAsync(int studentId)
        {
            return await _context.Matriculas
                .Where(m => m.IdAluno == studentId && m.IsActive && m.Status == EnrollmentStatus.Enrolled)
                .OrderByDescending(m => m.DataMatricula)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Matriculas
                .Where(m => m.IdCurso == courseId)
                .OrderByDescending(m => m.DataMatricula)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetActiveByCourseIdAsync(int courseId)
        {
            return await _context.Matriculas
                .Where(m => m.IdCurso == courseId && m.IsActive && m.Status == EnrollmentStatus.Enrolled)
                .OrderByDescending(m => m.DataMatricula)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId)
        {
            return await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdAluno == studentId && m.IdCurso == courseId);
        }

        public async Task<Enrollment?> GetActiveByStudentAndCourseAsync(int studentId, int courseId)
        {
            return await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdAluno == studentId && 
                                         m.IdCurso == courseId && 
                                         m.IsActive && 
                                         m.Status == EnrollmentStatus.Enrolled);
        }

        public async Task<Enrollment> CreateAsync(Enrollment enrollment)
        {
            // Garantir que novos registros tenham valores padrão corretos
            enrollment.CreatedAt = DateTime.UtcNow;
            enrollment.UpdatedAt = DateTime.UtcNow;
            enrollment.IsActive = true;
            enrollment.Status = EnrollmentStatus.Enrolled;

            _context.Matriculas.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<Enrollment?> UpdateAsync(Enrollment enrollment)
        {
            var existingEnrollment = await _context.Matriculas.FindAsync(enrollment.Id);
            if (existingEnrollment == null) return null;

            // Atualizar campos modificáveis
            existingEnrollment.IdCurso = enrollment.IdCurso;
            existingEnrollment.Status = enrollment.Status;
            existingEnrollment.Notes = enrollment.Notes;
            existingEnrollment.IsActive = enrollment.IsActive;
            // UpdatedAt será definido automaticamente pelo contexto

            _context.Matriculas.Update(existingEnrollment);
            await _context.SaveChangesAsync();
            return existingEnrollment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var enrollment = await _context.Matriculas.FindAsync(id);
            if (enrollment == null) return false;

            _context.Matriculas.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id, string? reason = null)
        {
            var enrollment = await _context.Matriculas.FindAsync(id);
            if (enrollment == null) return false;

            enrollment.SoftDelete(reason);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StudentExistsInCourseAsync(int studentId, int courseId)
        {
            return await _context.Matriculas
                .AnyAsync(m => m.IdAluno == studentId && m.IdCurso == courseId);
        }

        public async Task<bool> StudentActiveInCourseAsync(int studentId, int courseId)
        {
            return await _context.Matriculas
                .AnyAsync(m => m.IdAluno == studentId && 
                              m.IdCurso == courseId && 
                              m.IsActive && 
                              m.Status == EnrollmentStatus.Enrolled);
        }

        public async Task<int> GetCourseEnrollmentCountAsync(int courseId)
        {
            return await _context.Matriculas
                .CountAsync(m => m.IdCurso == courseId);
        }

        public async Task<int> GetActiveCourseEnrollmentCountAsync(int courseId)
        {
            return await _context.Matriculas
                .CountAsync(m => m.IdCurso == courseId && 
                               m.IsActive && 
                               m.Status == EnrollmentStatus.Enrolled);
        }

        public async Task<IEnumerable<Enrollment>> GetByStatusAsync(string status)
        {
            return await _context.Matriculas
                .Where(m => m.Status == status)
                .OrderByDescending(m => m.DataMatricula)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Matriculas
                .Where(m => m.DataMatricula >= startDate && m.DataMatricula <= endDate)
                .OrderByDescending(m => m.DataMatricula)
                .ToListAsync();
        }
    }
}