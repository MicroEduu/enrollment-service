using System.ComponentModel.DataAnnotations;

namespace EnrollmentService.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int IdAluno { get; set; }
        
        [Required]
        public int IdCurso { get; set; }
        
        [Required]
        public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    }
}
