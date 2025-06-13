using EnrollmentService.ExternalServices; 
 
namespace EnrollmentService.Services 
{ 
    public interface IAuthService 
    { 
        Task<UserDto?> GetUserByIdAsync(int userId); 
        Task<bool> IsUserStudentAsync(int userId); 
    } 
 
    public interface ICourseService 
    { 
        Task<CourseDto?> GetCourseByIdAsync(int courseId); 
        Task<bool> UpdateCourseEnrollmentCountAsync(int courseId, int newCount); 
    } 
} 
