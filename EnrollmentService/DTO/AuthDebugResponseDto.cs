namespace EnrollmentService.DTO
{
    /// <summary>
    /// Claim information for debugging
    /// </summary>
    public class ClaimDto
    {
        /// <summary>
        /// Claim type
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Claim value
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Authentication debug response
    /// </summary>
    public class AuthDebugResponseDto
    {
        /// <summary>
        /// Whether user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
        
        /// <summary>
        /// User name from identity
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// All claims in the token
        /// </summary>
        public List<ClaimDto> Claims { get; set; } = new();
        
        /// <summary>
        /// User ID from token
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// User role from token
        /// </summary>
        public string? Role { get; set; }
    }
}