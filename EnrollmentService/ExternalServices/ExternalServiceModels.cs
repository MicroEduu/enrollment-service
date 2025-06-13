namespace EnrollmentService.ExternalServices 
{ 
    public class UserDto 
    { 
        public int Id { get; set; } 
        public string Nome { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty; 
        public string Tipo { get; set; } = string.Empty; 
    } 
 
    public class CourseDto 
    { 
        public int Id { get; set; } 
        public string Titulo { get; set; } = string.Empty; 
        public string Descricao { get; set; } = string.Empty; 
        public int IdProfessor { get; set; } 
        public int QuantidadeInscritos { get; set; } 
    } 
} 
