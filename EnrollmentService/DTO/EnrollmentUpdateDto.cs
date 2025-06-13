using System.ComponentModel.DataAnnotations; 
 
namespace EnrollmentService.DTO 
{ 
    public class EnrollmentUpdateDto 
    { 
        [Required(ErrorMessage = "ID do curso e obrigatorio")] 
        public int IdCurso { get; set; } 
    } 
} 
