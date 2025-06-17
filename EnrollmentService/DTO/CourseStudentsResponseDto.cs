namespace EnrollmentService.DTO
{
    /// <summary>
    /// Response containing all students in a course
    /// </summary>
    public class CourseStudentsResponseDto
    {
        /// <summary>
        /// Course ID
        /// </summary>
        public int CourseId { get; set; }
        
        /// <summary>
        /// Course name
        /// </summary>
        public string CourseName { get; set; } = string.Empty;
        
        /// <summary>
        /// Course description
        /// </summary>
        public string CourseDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// Teacher ID
        /// </summary>
        public int TeacherId { get; set; }
        
        /// <summary>
        /// Total number of enrollments
        /// </summary>
        public int TotalEnrollments { get; set; }
        
        /// <summary>
        /// Total number of students
        /// </summary>
        public int TotalStudents { get; set; }
        
        /// <summary>
        /// List of students enrolled in the course
        /// </summary>
        public List<StudentInCourseDto> Students { get; set; } = new();
    }
}