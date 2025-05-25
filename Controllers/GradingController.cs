using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradingController : Controller
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public GradingController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult GetGradingById(int id)
        {
            var grading = _context.Grading.FirstOrDefault(g => g.id == id);

            if (grading == null)
            {
                return NotFound(new { message = "Không tìm thấy điểm với ID đã cho." });
            }

            return Ok(grading);
        }

        [HttpPost]
        [Authorize(Roles = "1,3")]
        public async Task<IActionResult> PostGrading([FromBody] Grading grading)
        {
            if (grading == null)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            try
            {
                _context.Grading.Add(grading);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Chấm điểm thành công.", data = grading });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }

        [Authorize(Roles = "1, 3")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGrand(int id, Grading grading)
        {
            if (id != grading.id)
            {
                return BadRequest();
            }

            _context.Entry(grading).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool GradingExists(int id)
        {
            return _context.Grading.Any(g => g.id == id);
        }
    }
}
