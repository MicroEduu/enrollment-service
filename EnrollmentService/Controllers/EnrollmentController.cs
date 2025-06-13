using Microsoft.AspNetCore.Mvc;
using EnrollmentService.DTO;
using EnrollmentService.Repositories;

namespace EnrollmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentRepository _repository;

        public EnrollmentController(IEnrollmentRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enrollments = await _repository.GetAllAsync();
            return Ok(enrollments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment == null) return NotFound();
            return Ok(enrollment);
        }
    }
}
