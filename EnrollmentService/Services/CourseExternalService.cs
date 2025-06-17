using EnrollmentService.ExternalServices;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace EnrollmentService.Services
{
    public class CourseExternalService : ICourseService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseExternalService(
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                AddJwtFromRequestToClient();

                var courseServiceUrl = _configuration["ExternalServices:CourseService"];
                var response = await _httpClient.GetAsync($"{courseServiceUrl}/api/Course/{courseId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<CourseDto>(json, new JsonSerializerOptions
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

        public async Task<bool> UpdateCourseEnrollmentCountAsync(int courseId, int newCount)
        {
            try
            {
                AddJwtFromRequestToClient();

                var courseServiceUrl = _configuration["ExternalServices:CourseService"];
                var updateData = new { QuantidadeInscritos = newCount };
                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"{courseServiceUrl}/api/Course/increment-subscriber/{courseId}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Adiciona o token JWT atual ao header Authorization do HttpClient
        private void AddJwtFromRequestToClient()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }
        }
    }
}
