using EnrollmentService.ExternalServices;
using System.Text.Json;

namespace EnrollmentService.Services
{
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(HttpClient httpClient, IConfiguration configuration, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        try
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            var authServiceUrl = _configuration["ExternalServices:AuthService"];
            var response = await _httpClient.GetAsync($"{authServiceUrl}/api/User/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            _logger.LogWarning("Falha ao consultar usuário {UserId}. Status: {StatusCode}", userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar usuário {UserId} no serviço externo", userId);
            return null;
        }
    }

        public async Task<bool> IsUserStudentAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Usuário {UserId} não encontrado no serviço externo", userId);
                    return false;
                }

                _logger.LogInformation("Dados do usuário {UserId}: Role={Role}, RoleName='{RoleName}'", userId, user.Role, user.RoleName);

                // CORREÇÃO: Verificar tanto pelo enum (3=Student) quanto pelo nome
                bool isStudent = user.Role == 3 || user.RoleName == "Student";

                _logger.LogInformation("Usuário {UserId} é estudante? {IsStudent}", userId, isStudent);
                return isStudent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se usuário {UserId} é estudante", userId);
                return false;
            }
        }
    }
}