using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnrollmentService.DTO;
using EnrollmentService.Repositories;
using EnrollmentService.Services;
using EnrollmentService.Models;
using System.Security.Claims;

namespace EnrollmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentRepository _repository;
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(
            IEnrollmentRepository repository,
            IAuthService authService,
            ICourseService courseService,
            ILogger<EnrollmentController> logger)
        {
            _repository = repository;
            _authService = authService;
            _courseService = courseService;
            _logger = logger;
        }

        /// <summary>
        /// Enroll a student in a course
        /// </summary>
        /// <param name="dto">Enrollment data</param>
        /// <returns>Enrollment confirmation</returns>
        /// <response code="200">Student successfully enrolled</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - invalid token</response>
        /// <response code="403">Forbidden - only students can enroll</response>
        /// <response code="404">Course not found</response>
        /// <response code="409">Student already enrolled in this course</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(EnrollmentSuccessResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 409)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new ErrorResponseDto { Message = "Invalid token" });

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != "Student")
                    return StatusCode(403, new ErrorResponseDto { 
                        Message = "Only students can enroll in courses", 
                        Details = $"Current role: {userRole}" 
                    });

                dto.IdAluno = userId;

                var course = await _courseService.GetCourseByIdAsync(dto.IdCurso);
                if (course == null)
                    return NotFound(new ErrorResponseDto { Message = $"Course with ID {dto.IdCurso} not found" });

                var existing = await _repository.GetByStudentAndCourseAsync(userId, dto.IdCurso);
                if (existing != null)
                    return Conflict(new ErrorResponseDto { Message = "Student is already enrolled in this course" });

                var enrollment = new Enrollment
                {
                    IdAluno = userId,
                    IdCurso = dto.IdCurso,
                    DataMatricula = DateTime.UtcNow
                };

                var created = await _repository.CreateAsync(enrollment);

                var count = await _repository.GetCourseEnrollmentCountAsync(dto.IdCurso);
                await _courseService.UpdateCourseEnrollmentCountAsync(dto.IdCurso, count);

                return Ok(new EnrollmentSuccessResponseDto
                {
                    Message = "Enrollment successful",
                    EnrollmentId = created.Id,
                    StudentId = userId,
                    CourseId = dto.IdCurso,
                    CourseName = course.Titulo,
                    EnrollmentDate = created.DataMatricula
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing enrollment");
                return StatusCode(500, new ErrorResponseDto { Message = "Internal error while processing enrollment" });
            }
        }

        /// <summary>
        /// Get all students enrolled in a specific course
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <returns>List of students enrolled in the course</returns>
        /// <response code="200">List of students retrieved successfully</response>
        /// <response code="401">Unauthorized - invalid token</response>
        /// <response code="403">Forbidden - only admins and teachers can access</response>
        /// <response code="404">Course not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("courses/{courseId}/students")]
        [Authorize]
        [ProducesResponseType(typeof(CourseStudentsResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        public async Task<IActionResult> GetStudentsByCourse(int courseId)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                _logger.LogInformation("Attempt to access students for course {CourseId} by user {UserId} with role {Role}", 
                    courseId, userId, userRole);

                if (userRole != "Admin" && userRole != "Teacher")
                {
                    _logger.LogWarning("Access denied: user with role '{Role}' attempted to access student list", userRole);
                    return StatusCode(403, new ErrorResponseDto
                    { 
                        Message = "Only administrators and teachers can view course students",
                        Details = $"Current role: {userRole}. Required roles: Admin, Teacher"
                    });
                }

                // Check if course exists
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null)
                {
                    return NotFound(new ErrorResponseDto { Message = $"Course with ID {courseId} not found" });
                }

                // If teacher, verify they own the course
                if (userRole == "Teacher" && int.Parse(userId!) != course.IdProfessor)
                {
                    _logger.LogWarning("Teacher {UserId} attempted to access course {CourseId} they don't own. Course teacher: {TeacherId}", 
                        userId, courseId, course.IdProfessor);
                    return StatusCode(403, new ErrorResponseDto
                    { 
                        Message = "Teachers can only view students from their own courses",
                        Details = $"Course teacher ID: {course.IdProfessor}"
                    });
                }

                var enrollments = await _repository.GetByCourseIdAsync(courseId);
                var studentIds = enrollments.Select(e => e.IdAluno).Distinct().ToList();
                var students = new List<StudentInCourseDto>();

                _logger.LogInformation("Found {Count} enrollments for course {CourseId}", enrollments.Count(), courseId);

                foreach (var id in studentIds)
                {
                    try
                    {
                        var user = await _authService.GetUserByIdAsync(id);
                        if (user != null && user.Role == 3) // 3 = Student
                        {
                            var studentEnrollment = enrollments.First(e => e.IdAluno == id);
                            students.Add(new StudentInCourseDto
                            {
                                Id = user.Id,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                                Email = user.Email,
                                Role = user.RoleName,
                                EnrollmentDate = studentEnrollment.DataMatricula,
                                EnrollmentId = studentEnrollment.Id,
                                IsActive = user.IsActive
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching data for student {StudentId}", id);
                        students.Add(new StudentInCourseDto
                        {
                            Id = id,
                            FirstName = "User",
                            LastName = "Not Found",
                            FullName = "User Not Found",
                            Email = "N/A",
                            Role = "Unknown",
                            EnrollmentDate = enrollments.First(e => e.IdAluno == id).DataMatricula,
                            EnrollmentId = enrollments.First(e => e.IdAluno == id).Id,
                            IsActive = false
                        });
                    }
                }

                return Ok(new CourseStudentsResponseDto
                {
                    CourseId = courseId,
                    CourseName = course.Titulo,
                    CourseDescription = course.Descricao,
                    TeacherId = course.IdProfessor,
                    TotalEnrollments = enrollments.Count(),
                    TotalStudents = students.Count,
                    Students = students.OrderBy(s => s.FirstName).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching students for course {CourseId}", courseId);
                return StatusCode(500, new ErrorResponseDto { Message = "Internal error while fetching course students" });
            }
        }

        /// <summary>
        /// Get all courses a student is enrolled in
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of courses the student is enrolled in</returns>
        /// <response code="200">List of courses retrieved successfully</response>
        /// <response code="401">Unauthorized - invalid token</response>
        /// <response code="403">Forbidden - insufficient permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("students/{studentId}/courses")]
        [Authorize]
        [ProducesResponseType(typeof(StudentEnrollmentsResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        public async Task<IActionResult> GetStudentEnrollments(int studentId)
        {
            try
            {
                var requesterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                _logger.LogInformation("Attempt to view enrollments for student {StudentId} by user {RequesterId} (Role: {Role})", 
                    studentId, requesterId, role);

                // Authorization check
                if (role == "Student" && requesterId != studentId)
                {
                    return StatusCode(403, new ErrorResponseDto { Message = "Students can only view their own enrollments" });
                }

                var enrollments = await _repository.GetByStudentIdAsync(studentId);
                var courses = new List<StudentCourseDto>();

                _logger.LogInformation("Found {Count} enrollments for student {StudentId}", enrollments.Count(), studentId);

                foreach (var enrollment in enrollments)
                {
                    try
                    {
                        var course = await _courseService.GetCourseByIdAsync(enrollment.IdCurso);
                        if (course != null)
                        {
                            // If teacher, only show courses they teach
                            if (role == "Teacher" && requesterId != course.IdProfessor)
                            {
                                _logger.LogDebug("Skipping course {CourseId} for teacher {RequesterId} - not their course", enrollment.IdCurso, requesterId);
                                continue;
                            }

                            courses.Add(new StudentCourseDto
                            {
                                EnrollmentId = enrollment.Id,
                                CourseId = course.Id,
                                CourseName = course.Titulo,
                                CourseDescription = course.Descricao,
                                TeacherId = course.IdProfessor,
                                EnrollmentDate = enrollment.DataMatricula,
                                TotalEnrollments = course.QuantidadeInscritos
                            });
                        }
                        else
                        {
                            // Course not found, but include enrollment with placeholder data
                            _logger.LogWarning("Course {CourseId} not found for enrollment {EnrollmentId}", enrollment.IdCurso, enrollment.Id);
                            courses.Add(new StudentCourseDto
                            {
                                EnrollmentId = enrollment.Id,
                                CourseId = enrollment.IdCurso,
                                CourseName = "Course Not Found",
                                CourseDescription = "Course data is not available",
                                TeacherId = 0,
                                EnrollmentDate = enrollment.DataMatricula,
                                TotalEnrollments = 0
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching course {CourseId} for student {StudentId}", enrollment.IdCurso, studentId);
                        
                        // Add placeholder for failed course lookup
                        courses.Add(new StudentCourseDto
                        {
                            EnrollmentId = enrollment.Id,
                            CourseId = enrollment.IdCurso,
                            CourseName = "Error Loading Course",
                            CourseDescription = "Failed to load course information",
                            TeacherId = 0,
                            EnrollmentDate = enrollment.DataMatricula,
                            TotalEnrollments = 0
                        });
                    }
                }

                return Ok(new StudentEnrollmentsResponseDto
                {
                    StudentId = studentId,
                    TotalCourses = courses.Count,
                    Courses = courses.OrderBy(c => c.CourseName).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching enrollments for student {StudentId}", studentId);
                return StatusCode(500, new ErrorResponseDto { Message = "Internal error while fetching student enrollments" });
            }
        }

        /// <summary>
        /// Debug authentication information (Admin only)
        /// </summary>
        /// <returns>Authentication debug information</returns>
        /// <response code="200">Debug information retrieved successfully</response>
        /// <response code="401">Unauthorized - invalid token</response>
        /// <response code="403">Forbidden - admin access required</response>
        [HttpGet("debug/auth")]
        [Authorize]
        [ProducesResponseType(typeof(AuthDebugResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        public IActionResult DebugAuth()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return StatusCode(403, new ErrorResponseDto { Message = "Debug endpoints are restricted to administrators only" });
            }

            var claims = User.Claims.Select(c => new ClaimDto { Type = c.Type, Value = c.Value }).ToList();
            return Ok(new AuthDebugResponseDto
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Name = User.Identity?.Name,
                Claims = claims,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        /// <summary>
        /// Debug external service information (Admin only)
        /// </summary>
        /// <param name="userId">User ID to debug</param>
        /// <returns>External service debug information</returns>
        /// <response code="200">Debug information retrieved successfully</response>
        /// <response code="401">Unauthorized - invalid token</response>
        /// <response code="403">Forbidden - admin access required</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("debug/external/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ExternalServiceDebugResponseDto), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 403)]
        [ProducesResponseType(typeof(ErrorResponseDto), 500)]
        public async Task<IActionResult> DebugExternal(int userId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return StatusCode(403, new ErrorResponseDto { Message = "Debug endpoints are restricted to administrators only" });
            }

            try
            {
                var user = await _authService.GetUserByIdAsync(userId);
                var isStudent = await _authService.IsUserStudentAsync(userId);
                return Ok(new ExternalServiceDebugResponseDto
                {
                    UserFound = user != null,
                    User = user,
                    IsStudentByService = isStudent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponseDto { Message = ex.Message });
            }
        }
    }
}