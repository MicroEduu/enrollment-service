using EnrollmentService.ExternalServices;
using System.Text.Json;

namespace EnrollmentService.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var authServiceUrl = _configuration["ExternalServices:AuthService"];
                var response = await _httpClient.GetAsync($"{authServiceUrl}/api/users/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsUserStudentAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            return user?.Tipo?.ToLower() == "aluno";
        }
    }
}
