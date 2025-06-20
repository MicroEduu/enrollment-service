# Guia de Testes - Enrollment Service API

## 🚀 **Como Executar**

### **Pré-requisitos:**
- .NET 6.0 ou superior
- SQLite (já incluído)

### **Executando a API:**
```bash
# Clone o repositório
git clone <seu-repositorio>
cd EnrollmentService

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run

# A API estará disponível em:
# - http://localhost:5263
# - Swagger UI: http://localhost:5263/swagger
```

### **Configuração:**
- O banco SQLite será criado automaticamente
- Configure o JWT SecretKey no `appsettings.json`
- Configure as URLs dos serviços externos (AuthService e CourseService)

## 🔗 **Endpoints Disponíveis**

### **1. Matricular Aluno** `POST /api/Enrollment`
- **Acesso:** Students only
- **Descrição:** Matricula um aluno em um curso e incrementa automaticamente o contador de inscrições do curso

```bash
curl -X 'POST' \
  'http://localhost:5263/api/Enrollment' \
  -H 'Authorization: Bearer SEU_TOKEN_STUDENT' \
  -H 'Content-Type: application/json' \
  -d '{
    "idAluno": 3,
    "idCurso": 1
  }'
```

**Resposta de Sucesso (200):**
```json
{
  "message": "Enrollment successful",
  "enrollmentId": 1,
  "studentId": 3,
  "courseId": 1,
  "courseName": "Introduction to Programming",
  "enrollmentDate": "2025-06-17T14:30:00"
}
```

**Funcionalidades:**
- ✅ Verifica se o curso existe
- ✅ Verifica se o aluno já está matriculado
- ✅ Cria a matrícula no banco local
- ✅ **Incrementa automaticamente o contador de inscrições no serviço de cursos**

### **2. Listar Alunos do Curso** `GET /api/Enrollment/courses/{courseId}/students`
- **Acesso:** Admin e Teacher (apenas próprios cursos)
- **Descrição:** Lista todos os alunos matriculados em um curso

```bash
curl -X 'GET' \
  'http://localhost:5263/api/Enrollment/courses/1/students' \
  -H 'Authorization: Bearer SEU_TOKEN_ADMIN_OU_TEACHER'
```

**Resposta de Sucesso (200):**
```json
{
  "courseId": 1,
  "courseName": "Introduction to Programming",
  "courseDescription": "Basic programming concepts",
  "teacherId": 2,
  "totalEnrollments": 3,
  "totalStudents": 3,
  "students": [
    {
      "id": 3,
      "firstName": "Student",
      "lastName": "Name",
      "fullName": "Student Name",
      "email": "student@email.com",
      "role": "Student",
      "enrollmentDate": "2025-06-17T14:30:00",
      "enrollmentId": 1,
      "isActive": true
    }
  ]
}
```

### **3. Listar Cursos do Aluno** `GET /api/Enrollment/students/{studentId}/courses`
- **Acesso:** Admin, Teacher (próprios cursos), Student (próprios cursos)
- **Descrição:** Lista todos os cursos em que um aluno está matriculado

```bash
curl -X 'GET' \
  'http://localhost:5263/api/Enrollment/students/3/courses' \
  -H 'Authorization: Bearer SEU_TOKEN'
```

**Resposta de Sucesso (200):**
```json
{
  "studentId": 3,
  "totalCourses": 2,
  "courses": [
    {
      "enrollmentId": 1,
      "courseId": 1,
      "courseName": "Introduction to Programming",
      "courseDescription": "Basic programming concepts",
      "teacherId": 2,
      "enrollmentDate": "2025-06-17T14:30:00",
      "totalEnrollments": 15
    }
  ]
}
```

### **4. Debug de Autenticação** `GET /api/Enrollment/debug/auth`
- **Acesso:** Admin only
- **Descrição:** Informações de debug da autenticação

```bash
curl -X 'GET' \
  'http://localhost:5263/api/Enrollment/debug/auth' \
  -H 'Authorization: Bearer SEU_TOKEN_ADMIN'
```

### **5. Debug de Serviço Externo** `GET /api/Enrollment/debug/external/{userId}`
- **Acesso:** Admin only
- **Descrição:** Informações de debug do serviço externo

```bash
curl -X 'GET' \
  'http://localhost:5263/api/Enrollment/debug/external/3' \
  -H 'Authorization: Bearer SEU_TOKEN_ADMIN'
```

## 🔐 **Controle de Acesso**

| Endpoint | Student | Teacher | Admin |
|----------|---------|---------|-------|
| POST /api/Enrollment | ✅ | ❌ | ❌ |
| GET /courses/{id}/students | ❌ | ✅ (próprios cursos) | ✅ |
| GET /students/{id}/courses | ✅ (próprios) | ✅ (próprios cursos) | ✅ |
| GET /debug/* | ❌ | ❌ | ✅ |

## 📝 **Códigos de Resposta**

### **Sucesso:**
- `200 OK` - Operação realizada com sucesso

### **Erro do Cliente:**
- `400 Bad Request` - Dados inválidos na requisição
- `401 Unauthorized` - Token ausente ou inválido
- `403 Forbidden` - Sem permissão para acessar o recurso
- `404 Not Found` - Recurso não encontrado
- `409 Conflict` - Conflito (ex: aluno já matriculado)

### **Erro do Servidor:**
- `500 Internal Server Error` - Erro interno do servidor

## 🧪 **Cenários de Teste**

### **✅ Cenários de Sucesso:**
1. Student se matricula em curso válido (contador incrementado automaticamente)
2. Admin visualiza alunos de qualquer curso
3. Teacher visualiza alunos dos próprios cursos
4. Student visualiza próprias matrículas
5. Admin acessa endpoints de debug

### **❌ Cenários de Erro:**
1. Student tenta matricular em curso inexistente (404)
2. Student tenta se matricular novamente no mesmo curso (409)
3. Teacher tenta visualizar alunos de curso de outro professor (403)
4. Student tenta visualizar matrículas de outro aluno (403)
5. Student/Teacher tenta acessar debug (403)
6. Requisição sem token (401)
7. Token inválido ou expirado (401)

## 🛠 **Troubleshooting**

### **Erro 403 "Only students can enroll":**
- Verificar se o token tem role "Student"
- Usar endpoint debug para verificar claims

### **Erro 404 "Course not found":**
- Verificar se o curso existe na API de cursos
- Verificar conectividade entre APIs

### **Erro 409 "Already enrolled":**
- Aluno já está matriculado no curso
- Verificar base de dados de matrículas

### **Erro 403 "Teachers can only view their own courses":**
- Professor está tentando acessar curso de outro professor
- Verificar se o `idTeacher` do curso corresponde ao ID do professor logado

## 📊 **Monitoramento**

A API gera logs detalhados para:
- Tentativas de matrícula
- Verificações de autorização
- Chamadas para APIs externas
- Erros e exceções

Use os logs para diagnosticar problemas de integração e autorização.

## 🔄 **Integrações**

### **Serviços Externos:**
- **AuthService**: Validação de usuários e roles
- **CourseService**: Consulta e atualização de cursos

### **Processo de Matrícula:**
1. Student faz requisição com token JWT
2. Sistema valida token e verifica role "Student"
3. Verifica se curso existe no CourseService
4. Verifica se aluno já está matriculado
5. Cria matrícula no banco local
6. **Atualiza contador de inscrições no CourseService**
7. Retorna confirmação da matrícula