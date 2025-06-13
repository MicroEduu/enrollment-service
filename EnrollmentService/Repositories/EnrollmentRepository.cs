using EnrollmentService.Data;
using EnrollmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace EnrollmentService.Repositories
{
    public interface IEnrollmentRepository
    {
        Task<IEnumerable<Enrollment>> GetAllAsync();
        Task<Enrollment?> GetByIdAsync(int id);
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId);
        Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId);
        Task<Enrollment> CreateAsync(Enrollment enrollment);
        Task<Enrollment?> UpdateAsync(Enrollment enrollment);
        Task<bool> DeleteAsync(int id);
        Task<bool> StudentExistsInCourseAsync(int studentId, int courseId);
        Task<int> GetCourseEnrollmentCountAsync(int courseId);
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

        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            return await _context.Matriculas.FindAsync(id);
        }

        public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.Matriculas
                .Where(m => m.IdAluno == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Matriculas
                .Where(m => m.IdCurso == courseId)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetByStudentAndCourseAsync(int studentId, int courseId)
        {
            return await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdAluno == studentId && m.IdCurso == courseId);
        }

        public async Task<Enrollment> CreateAsync(Enrollment enrollment)
        {
            _context.Matriculas.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<Enrollment?> UpdateAsync(Enrollment enrollment)
        {
            var existingEnrollment = await _context.Matriculas.FindAsync(enrollment.Id);
            if (existingEnrollment == null) return null;

            existingEnrollment.IdCurso = enrollment.IdCurso;

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

        public async Task<bool> StudentExistsInCourseAsync(int studentId, int courseId)
        {
            return await _context.Matriculas
                .AnyAsync(m => m.IdAluno == studentId && m.IdCurso == courseId);
        }

        public async Task<int> GetCourseEnrollmentCountAsync(int courseId)
        {
            return await _context.Matriculas
                .CountAsync(m => m.IdCurso == courseId);
        }
    }
}