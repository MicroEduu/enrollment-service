namespace EnrollmentService.DTO
{
    /// <summary>
    /// Response containing all courses for a student
    /// </summary>
    public class StudentEnrollmentsResponseDto
    {
        /// <summary>
        /// Student ID
        /// </summary>
        public int StudentId { get; set; }
        
        /// <summary>
        /// Total number of courses the student is enrolled in
        /// </summary>
        public int TotalCourses { get; set; }
        
        /// <summary>
        /// List of courses the student is enrolled in
        /// </summary>
        public List<StudentCourseDto> Courses { get; set; } = new();
    }
}