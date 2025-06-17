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
 
    // CORREÇÃO: Modelo que corresponde EXATAMENTE à resposta da API de cursos
    public class CourseDto 
    { 
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int IdTeacher { get; set; } // Campo "idTeacher" da API de cursos
        public int NumberSubscribers { get; set; } // Campo "numberSubscribers" da API de cursos  
        public DateTime CreatedAt { get; set; } // Campo "createdAt" da API de cursos
        
        // Propriedades auxiliares para compatibilidade com código existente
        public string Titulo => Title;
        public string Descricao => Description;
        public int QuantidadeInscritos => NumberSubscribers;
        public int IdProfessor => IdTeacher; // Mapear IdTeacher para IdProfessor
    } 
}