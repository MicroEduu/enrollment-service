namespace EnrollmentService.DTO
{
    /// <summary>
    /// Success response for enrollment
    /// </summary>
    public class EnrollmentSuccessResponseDto
    {
        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Created enrollment ID
        /// </summary>
        public int EnrollmentId { get; set; }
        
        /// <summary>
        /// Student ID
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// Course ID
        /// </summary>
        public int CourseId { get; set; }
        
        /// <summary>
        /// Course name
        /// </summary>
        public string CourseName { get; set; } = string.Empty;
        
        /// <summary>
        /// Enrollment date and time
        /// </summary>
        public DateTime EnrollmentDate { get; set; }
    }
}