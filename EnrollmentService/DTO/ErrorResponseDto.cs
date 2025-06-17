namespace EnrollmentService.DTO
{
    /// <summary>
    /// Error response
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional error details
        /// </summary>
        public string? Details { get; set; }
    }
}