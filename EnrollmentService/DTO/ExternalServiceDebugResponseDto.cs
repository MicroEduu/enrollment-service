namespace EnrollmentService.DTO
{
    /// <summary>
    /// External service debug response
    /// </summary>
    public class ExternalServiceDebugResponseDto
    {
        /// <summary>
        /// Whether user was found in external service
        /// </summary>
        public bool UserFound { get; set; }
        
        /// <summary>
        /// User data from external service
        /// </summary>
        public object? User { get; set; }
        
        /// <summary>
        /// Whether external service considers user a student
        /// </summary>
        public bool IsStudentByService { get; set; }
    }
}