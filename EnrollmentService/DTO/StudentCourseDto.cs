namespace EnrollmentService.DTO
{
    /// <summary>
    /// Course information for a student
    /// </summary>
    public class StudentCourseDto
    {
        /// <summary>
        /// Enrollment record ID
        /// </summary>
        public int EnrollmentId { get; set; }
        
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
        /// Date when student enrolled
        /// </summary>
        public DateTime EnrollmentDate { get; set; }
        
        /// <summary>
        /// Total number of students enrolled in this course
        /// </summary>
        public int TotalEnrollments { get; set; }
    }
}