using EnrollmentService.ExternalServices;
using System.Text.Json;
using System.Text;

namespace EnrollmentService.Services
{
    public class CourseExternalService : ICourseService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CourseExternalService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var courseServiceUrl = _configuration["ExternalServices:CourseService"];
                var response = await _httpClient.GetAsync($"{courseServiceUrl}/api/courses/{courseId}");
                
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
                var courseServiceUrl = _configuration["ExternalServices:CourseService"];
                var updateData = new { QuantidadeInscritos = newCount };
                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{courseServiceUrl}/api/courses/{courseId}/enrollment-count", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
