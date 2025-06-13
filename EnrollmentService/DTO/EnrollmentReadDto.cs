namespace EnrollmentService.DTO 
{ 
    public class EnrollmentReadDto 
    { 
        public int Id { get; set; } 
        public int IdAluno { get; set; } 
        public int IdCurso { get; set; } 
        public DateTime DataMatricula { get; set; } 
        public string? NomeAluno { get; set; } 
        public string? TituloCurso { get; set; } 
    } 
} 
