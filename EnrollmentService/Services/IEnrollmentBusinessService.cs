using EnrollmentService.DTO; 
 
namespace EnrollmentService.Services 
{ 
    public interface IEnrollmentBusinessService 
    { 
        Task<IEnumerable<EnrollmentReadDto>> GetAllEnrollmentsAsync(); 
        Task<EnrollmentReadDto?> GetEnrollmentByIdAsync(int id); 
        Task<IEnumerable<EnrollmentReadDto>> GetEnrollmentsByStudentAsync(int studentId); 
        Task<IEnumerable<EnrollmentReadDto>> GetEnrollmentsByCourseAsync(int courseId); 
        Task<EnrollmentReadDto?> CreateEnrollmentAsync(EnrollmentCreateDto enrollmentDto); 
        Task<EnrollmentReadDto?> UpdateEnrollmentAsync(int id, EnrollmentUpdateDto enrollmentDto); 
        Task<bool> DeleteEnrollmentAsync(int id); 
    } 
} 
