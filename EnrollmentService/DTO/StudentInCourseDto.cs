namespace EnrollmentService.DTO
{
    /// <summary>
    /// Student information in a course
    /// </summary>
    public class StudentInCourseDto
    {
        /// <summary>
        /// Student ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Student first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Student last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// Student full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Student email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Student role
        /// </summary>
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Date when student enrolled in the course
        /// </summary>
        public DateTime EnrollmentDate { get; set; }
        
        /// <summary>
        /// Enrollment record ID
        /// </summary>
        public int EnrollmentId { get; set; }
        
        /// <summary>
        /// Whether the student account is active
        /// </summary>
        public bool IsActive { get; set; }
    }
}