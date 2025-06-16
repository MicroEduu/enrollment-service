using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnrollmentService.DTO;
using EnrollmentService.Repositories;
using EnrollmentService.Services;
using EnrollmentService.Models;
using System.Security.Claims;

namespace EnrollmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentRepository _repository;
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(
            IEnrollmentRepository repository,
            IAuthService authService,
            ICourseService courseService,
            ILogger<EnrollmentController> logger)
        {
            _repository = repository;
            _authService = authService;
            _courseService = courseService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
        {
            try
            {
                _logger.LogInformation(">>> Iniciando processo de matrícula.");

                // Pega o ID do usuário a partir do token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Token JWT não possui 'NameIdentifier'.");
                    return Unauthorized("Token inválido: ID ausente");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Claim 'NameIdentifier' não é um número inteiro: {Value}", userIdClaim);
                    return Unauthorized("ID do usuário inválido");
                }

                // VERIFICAÇÃO DE ROLE PRINCIPAL: Usar token JWT como fonte da verdade
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                _logger.LogInformation("Usuário {UserId} autenticado com role: {Role}", userId, userRole);

                if (userRole != "Student")
                {
                    _logger.LogWarning("❌ Acesso negado: usuário {UserId} não é estudante. Role atual: {Role}", userId, userRole);
                    return StatusCode(403, new { 
                        message = "Apenas alunos podem se matricular",
                        currentRole = userRole,
                        requiredRole = "Student"
                    });
                }

                _logger.LogInformation("✅ Verificação de role aprovada: usuário {UserId} é Student", userId);

                // Usar sempre o ID do token por segurança
                dto.IdAluno = userId;

                // Verificação informativa via serviço externo (não bloqueia)
                try
                {
                    var user = await _authService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        _logger.LogInformation("Dados do serviço externo - Nome: {Nome}, Email: {Email}, Role: {Role}, RoleName: {RoleName}", 
                            user.FirstName, user.Email, user.Role, user.RoleName);
                        
                        var isStudentExternal = await _authService.IsUserStudentAsync(userId);
                        if (!isStudentExternal)
                        {
                            _logger.LogWarning("⚠️  INCONSISTÊNCIA: Token JWT = 'Student', Serviço externo Role = {Role} ({RoleName}). Priorizando token JWT.", user.Role, user.RoleName);
                        }
                        else
                        {
                            _logger.LogInformation("✅ Serviço externo confirma: usuário é estudante (Role={Role})", user.Role);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Usuário {UserId} não encontrado no serviço externo, mas token JWT é válido", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao consultar serviço externo. Continuando baseado no token JWT.");
                }

                // Verifica se o curso existe
                var course = await _courseService.GetCourseByIdAsync(dto.IdCurso);
                if (course == null)
                {
                    _logger.LogWarning("Curso {CursoId} não encontrado", dto.IdCurso);
                    return NotFound($"Curso com ID {dto.IdCurso} não encontrado");
                }

                _logger.LogInformation("Curso encontrado: {CourseTitle} (ID: {CourseId})", course.Titulo, course.Id);

                // Verifica se já está matriculado
                var existingEnrollment = await _repository.GetByStudentAndCourseAsync(userId, dto.IdCurso);
                if (existingEnrollment != null)
                {
                    _logger.LogWarning("Usuário {UserId} já está matriculado no curso {CursoId}", userId, dto.IdCurso);
                    return Conflict(new { 
                        message = "O aluno já está matriculado neste curso",
                        existingEnrollmentId = existingEnrollment.Id,
                        enrollmentDate = existingEnrollment.DataMatricula
                    });
                }

                // Cria a matrícula
                var enrollment = new Enrollment
                {
                    IdAluno = userId,
                    IdCurso = dto.IdCurso,
                    DataMatricula = DateTime.UtcNow
                };

                var createdEnrollment = await _repository.CreateAsync(enrollment);
                _logger.LogInformation("✅ Matrícula criada: ID={EnrollmentId}, Aluno={StudentId}, Curso={CourseId}", 
                    createdEnrollment.Id, userId, dto.IdCurso);

                // Atualiza contador de inscritos no curso
                try
                {
                    var count = await _repository.GetCourseEnrollmentCountAsync(dto.IdCurso);
                    var updateSuccess = await _courseService.UpdateCourseEnrollmentCountAsync(dto.IdCurso, count);

                    if (updateSuccess)
                    {
                        _logger.LogInformation("✅ Contador atualizado: curso {CourseId} tem {Count} inscritos", dto.IdCurso, count);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️  Falha ao atualizar contador para o curso {CourseId}", dto.IdCurso);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar contador de inscritos para o curso {CourseId}", dto.IdCurso);
                }

                _logger.LogInformation("🎉 Matrícula concluída com sucesso!");
                
                return Ok(new { 
                    message = "Matrícula realizada com sucesso",
                    enrollmentId = createdEnrollment.Id,
                    studentId = userId,
                    courseId = dto.IdCurso,
                    courseName = course.Titulo,
                    enrollmentDate = createdEnrollment.DataMatricula
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro grave ao processar matrícula");
                return StatusCode(500, new { 
                    message = "Erro interno do servidor ao processar matrícula",
                    error = ex.Message 
                });
            }
        }

        // Endpoint para debug
        [HttpGet("debug/auth")]
        [Authorize]
        public IActionResult DebugAuth()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated,
                name = User.Identity?.Name,
                authType = User.Identity?.AuthenticationType,
                claims = claims,
                isInStudentRole = User.IsInRole("Student"),
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                roleFromClaim = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        // Endpoint para testar serviço externo
        [HttpGet("debug/external/{userId}")]
        [Authorize]
        public async Task<IActionResult> DebugExternal(int userId)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(userId);
                var isStudent = await _authService.IsUserStudentAsync(userId);
                
                return Ok(new
                {
                    userFound = user != null,
                    user = user,
                    isStudentByService = isStudent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}