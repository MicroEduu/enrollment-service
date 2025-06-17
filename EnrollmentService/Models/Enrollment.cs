using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnrollmentService.Models
{
    /// <summary>
    /// Represents a student enrollment in a course
    /// </summary>
    [Table("Enrollments")]
    public class Enrollment
    {
        /// <summary>
        /// Unique identifier for the enrollment
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Student ID (foreign key to User service)
        /// </summary>
        [Required(ErrorMessage = "Student ID is required")]
        [Column("StudentId")]
        public int IdAluno { get; set; }

        /// <summary>
        /// Course ID (foreign key to Course service)
        /// </summary>
        [Required(ErrorMessage = "Course ID is required")]
        [Column("CourseId")]
        public int IdCurso { get; set; }

        /// <summary>
        /// Date and time when the enrollment was created
        /// </summary>
        [Required]
        [Column("EnrollmentDate")]
        public DateTime DataMatricula { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the enrollment record was created
        /// </summary>
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the enrollment record was last updated
        /// </summary>
        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the enrollment is active (soft delete support)
        /// </summary>
        [Required]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Additional notes about the enrollment
        /// </summary>
        [MaxLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// Enrollment status (Enrolled, Withdrawn, Completed, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("Status")]
        public string Status { get; set; } = "Enrolled";

        // Navigation properties (for potential future use with EF Core relationships)

        /// <summary>
        /// Student information (not mapped - comes from external service)
        /// </summary>
        [NotMapped]
        public string? StudentName { get; set; }

        /// <summary>
        /// Course information (not mapped - comes from external service)
        /// </summary>
        [NotMapped]
        public string? CourseName { get; set; }

        /// <summary>
        /// Teacher information (not mapped - comes from external service)
        /// </summary>
        [NotMapped]
        public string? TeacherName { get; set; }

        // Helper methods

        /// <summary>
        /// Checks if the enrollment is currently active
        /// </summary>
        /// <returns>True if enrollment is active and status is "Enrolled"</returns>
        public bool IsCurrentlyEnrolled()
        {
            return IsActive && Status.Equals("Enrolled", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Marks the enrollment as withdrawn
        /// </summary>
        /// <param name="reason">Reason for withdrawal</param>
        public void Withdraw(string? reason = null)
        {
            Status = "Withdrawn";
            UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
            {
                Notes = $"Withdrawn: {reason}. Previous notes: {Notes}".Trim();
            }
        }

        /// <summary>
        /// Marks the enrollment as completed
        /// </summary>
        public void Complete()
        {
            Status = "Completed";
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the enrollment
        /// </summary>
        /// <param name="reason">Reason for deletion</param>
        public void SoftDelete(string? reason = null)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
            {
                Notes = $"Deleted: {reason}. Previous notes: {Notes}".Trim();
            }
        }

        /// <summary>
        /// Updates the timestamp when the record is modified
        /// </summary>
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the duration of the enrollment in days
        /// </summary>
        /// <returns>Number of days since enrollment</returns>
        public int GetEnrollmentDurationInDays()
        {
            return (DateTime.UtcNow - DataMatricula).Days;
        }

        /// <summary>
        /// Returns a formatted string representation of the enrollment
        /// </summary>
        /// <returns>Formatted enrollment information</returns>
        public override string ToString()
        {
            return $"Enrollment {Id}: Student {IdAluno} in Course {IdCurso} ({Status}) - {DataMatricula:yyyy-MM-dd}";
        }
    }

    /// <summary>
    /// Enumeration for enrollment statuses
    /// </summary>
    public static class EnrollmentStatus
    {
        public const string Enrolled = "Enrolled";
        public const string Withdrawn = "Withdrawn";
        public const string Completed = "Completed";
        public const string Suspended = "Suspended";
        public const string Transferred = "Transferred";
    }
}