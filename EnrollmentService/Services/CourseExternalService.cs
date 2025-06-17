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
        private readonly ILogger<CourseExternalService> _logger;

        public CourseExternalService(
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CourseExternalService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            try
            {
                AddJwtFromRequestToClient();
                var courseServiceUrl = _configuration["ExternalServices:CourseService"];
                
                if (string.IsNullOrEmpty(courseServiceUrl))
                {
                    _logger.LogError("CourseService URL not configured");
                    return null;
                }
                
                _logger.LogInformation("Fetching course {CourseId} from service: {Url}", courseId, courseServiceUrl);
                
                var response = await _httpClient.GetAsync($"{courseServiceUrl}/api/Course/{courseId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Course service response for course {CourseId}: {Json}", courseId, json);
                    
                    var course = JsonSerializer.Deserialize<CourseDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (course != null)
                    {
                        _logger.LogInformation("Successfully loaded course: ID={Id}, Title='{Title}', Teacher={TeacherId}, Subscribers={Subscribers}", 
                            course.Id, course.Title, course.IdTeacher, course.NumberSubscribers);
                    }
                    else
                    {
                        _logger.LogWarning("Course {CourseId} deserialization returned null", courseId);
                    }

                    return course;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to fetch course {CourseId}. Status: {StatusCode}, Content: {Content}", 
                        courseId, response.StatusCode, errorContent);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching course {CourseId} from external service", courseId);
                return null;
            }
        }

        public async Task<bool> UpdateCourseEnrollmentCountAsync(int courseId, int newCount)
        {
            try
            {
                AddJwtFromRequestToClient();
                var courseServiceUrl = _configuration["ExternalServices:CourseService"];
                
                if (string.IsNullOrEmpty(courseServiceUrl))
                {
                    _logger.LogError("CourseService URL not configured");
                    return false;
                }
                
                _logger.LogInformation("Attempting to increment subscriber count for course {CourseId}", courseId);
                
                // Simplesmente tentar incrementar (assumindo que é uma nova matrícula)
                var response = await _httpClient.PatchAsync($"{courseServiceUrl}/api/Course/increment-subscriber/{courseId}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully incremented subscriber count for course {CourseId}", courseId);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to increment subscriber count for course {CourseId}. Status: {StatusCode}, Content: {Content}", 
                        courseId, response.StatusCode, errorContent);
                    
                    // Tentar método alternativo se o incremento falhar
                    return await TryAlternativeUpdate(courseServiceUrl, courseId, newCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enrollment count for course {CourseId}", courseId);
                return false;
            }
        }

        private async Task<bool> TryAlternativeUpdate(string courseServiceUrl, int courseId, int newCount)
        {
            try
            {
                _logger.LogInformation("Trying alternative update method for course {CourseId}", courseId);
                
                // Tentar diferentes estruturas de dados
                var updateData = new { NumberSubscribers = newCount };
                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Tentar diferentes endpoints
                var endpoints = new string[]
                {
                    $"{courseServiceUrl}/api/Course/{courseId}/subscribers",
                    $"{courseServiceUrl}/api/Course/{courseId}/enrollment-count",
                    $"{courseServiceUrl}/api/Course/{courseId}"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await _httpClient.PutAsync(endpoint, content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Successfully updated course {CourseId} using endpoint {Endpoint}", courseId, endpoint);
                            return true;
                        }
                        else
                        {
                            _logger.LogDebug("Failed to update course {CourseId} using endpoint {Endpoint}. Status: {StatusCode}", 
                                courseId, endpoint, response.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Exception trying endpoint {Endpoint} for course {CourseId}", endpoint, courseId);
                    }
                }

                _logger.LogWarning("All alternative update methods failed for course {CourseId}", courseId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in alternative update for course {CourseId}", courseId);
                return false;
            }
        }

        // Adiciona o token JWT atual ao header Authorization do HttpClient
        private void AddJwtFromRequestToClient()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var cleanToken = token.Replace("Bearer ", "");
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", cleanToken);
                    
                    _logger.LogDebug("JWT token added to HttpClient headers for course service call");
                }
                else
                {
                    _logger.LogWarning("No Authorization header found in current request");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding JWT token to HttpClient headers");
            }
        }
    }
}