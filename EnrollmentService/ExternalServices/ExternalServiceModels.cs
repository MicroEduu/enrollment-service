namespace EnrollmentService.ExternalServices 
{ 
    // Modelo que corresponde ao UserDTO do AuthService
    public class UserDto 
    { 
        public int Id { get; set; } 
        public string Email { get; set; } = string.Empty; 
        public string FirstName { get; set; } = string.Empty; 
        public string LastName { get; set; } = string.Empty; 
        
        // CORREÇÃO: Role é um enum no AuthService, não string
        public int Role { get; set; } // 1=Admin, 2=Teacher, 3=Student
        
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        
        // Propriedade auxiliar para facilitar verificação
        public string RoleName => Role switch
        {
            1 => "Admin",
            2 => "Teacher", 
            3 => "Student",
            _ => "Unknown"
        };
        
        // Para compatibilidade com código antigo
        public string Nome => FirstName;
        public string Tipo => RoleName;
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