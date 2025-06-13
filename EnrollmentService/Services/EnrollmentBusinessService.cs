using EnrollmentService.DTO;
using EnrollmentService.Models;
using EnrollmentService.Repositories;

namespace EnrollmentService.Services
{
    public class EnrollmentBusinessService : IEnrollmentBusinessService
    {
        private readonly IEnrollmentRepository _repository;
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;

        public EnrollmentBusinessService(
            IEnrollmentRepository repository,
            IAuthService authService,
            ICourseService courseService)
        {
            _repository = repository;
            _authService = authService;
            _courseService = courseService;
        }

        public async Task<IEnumerable<EnrollmentReadDto>> GetAllEnrollmentsAsync()
        {
            var enrollments = await _repository.GetAllAsync();
            return await EnrichEnrollmentsAsync(enrollments);
        }

        public async Task<EnrollmentReadDto?> GetEnrollmentByIdAsync(int id)
        {
            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment == null) return null;
            return await EnrichEnrollmentAsync(enrollment);
        }

        public async Task<IEnumerable<EnrollmentReadDto>> GetEnrollmentsByStudentAsync(int studentId)
        {
            var enrollments = await _repository.GetByStudentIdAsync(studentId);
            return await EnrichEnrollmentsAsync(enrollments);
        }

        public async Task<IEnumerable<EnrollmentReadDto>> GetEnrollmentsByCourseAsync(int courseId)
        {
            var enrollments = await _repository.GetByCourseIdAsync(courseId);
            return await EnrichEnrollmentsAsync(enrollments);
        }

        public async Task<EnrollmentReadDto?> CreateEnrollmentAsync(EnrollmentCreateDto enrollmentDto)
        {
            if (await _authService.IsUserStudentAsync(enrollmentDto.IdAluno))
                throw new InvalidOperationException("Apenas alunos podem se matricular em cursos");

            var course = await _courseService.GetCourseByIdAsync(enrollmentDto.IdCurso);
            if (course == null)
                throw new InvalidOperationException("Curso nao encontrado");

            if (await _repository.StudentExistsInCourseAsync(enrollmentDto.IdAluno, enrollmentDto.IdCurso))
                throw new InvalidOperationException("Aluno ja esta matriculado neste curso");

            var enrollment = new Enrollment
            {
                IdAluno = enrollmentDto.IdAluno,
                IdCurso = enrollmentDto.IdCurso,
                DataMatricula = DateTime.UtcNow
            };

            var createdEnrollment = await _repository.CreateAsync(enrollment);
            var currentCount = await _repository.GetCourseEnrollmentCountAsync(enrollmentDto.IdCurso);
            await _courseService.UpdateCourseEnrollmentCountAsync(enrollmentDto.IdCurso, currentCount);
            return await EnrichEnrollmentAsync(createdEnrollment);
        }

        public async Task<EnrollmentReadDto?> UpdateEnrollmentAsync(int id, EnrollmentUpdateDto enrollmentDto)
        {
            var existingEnrollment = await _repository.GetByIdAsync(id);
            if (existingEnrollment == null) throw new InvalidOperationException("Matricula nao encontrada");

            var course = await _courseService.GetCourseByIdAsync(enrollmentDto.IdCurso);
            if (course == null) throw new InvalidOperationException("Curso nao encontrado");

            var enrollment = new Enrollment
            {
                Id = id,
                IdAluno = existingEnrollment.IdAluno,
                IdCurso = enrollmentDto.IdCurso,
                DataMatricula = existingEnrollment.DataMatricula
            };

            var updatedEnrollment = await _repository.UpdateAsync(enrollment);
            return updatedEnrollment == null ? null : await EnrichEnrollmentAsync(updatedEnrollment);
        }

        public async Task<bool> DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment == null) return false;
            var result = await _repository.DeleteAsync(id);
            if (result)
            {
                var currentCount = await _repository.GetCourseEnrollmentCountAsync(enrollment.IdCurso);
                await _courseService.UpdateCourseEnrollmentCountAsync(enrollment.IdCurso, currentCount);
            }
            return result;
        }

        private async Task<EnrollmentReadDto> EnrichEnrollmentAsync(Enrollment enrollment)
        {
            var user = await _authService.GetUserByIdAsync(enrollment.IdAluno);
            var course = await _courseService.GetCourseByIdAsync(enrollment.IdCurso);
            return new EnrollmentReadDto
            {
                Id = enrollment.Id,
                IdAluno = enrollment.IdAluno,
                IdCurso = enrollment.IdCurso,
                DataMatricula = enrollment.DataMatricula,
                NomeAluno = user?.Nome,
                TituloCurso = course?.Titulo
            };
        }

        private async Task<IEnumerable<EnrollmentReadDto>> EnrichEnrollmentsAsync(IEnumerable<Enrollment> enrollments)
        {
            var enrichedList = new List<EnrollmentReadDto>();
            foreach (var enrollment in enrollments)
                enrichedList.Add(await EnrichEnrollmentAsync(enrollment));
            return enrichedList;
        }
    }
}
