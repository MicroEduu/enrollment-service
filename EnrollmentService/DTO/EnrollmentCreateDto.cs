using System.ComponentModel.DataAnnotations; 
 
namespace EnrollmentService.DTO 
{ 
    public class EnrollmentCreateDto 
    { 
        [Required(ErrorMessage = "ID do aluno e obrigatorio")] 
        public int IdAluno { get; set; } 
 
        [Required(ErrorMessage = "ID do curso e obrigatorio")] 
        public int IdCurso { get; set; } 
    } 
} 
