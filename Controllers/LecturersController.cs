using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturersController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public LecturersController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        [Authorize(Roles = "1,2,3")]
        public async Task<ActionResult<IEnumerable<Lecturers>>> GetLecturers()
        {
            return await _context.Lecturers.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2, 3")]
        public IActionResult GetLecturerById(int id)
        {
            var lecturer = _context.Lecturers.FirstOrDefault(s => s.lecturer_id == id);

            if (lecturer == null)
            {
                return NotFound(new { message = "Không tìm thấy giáo viên với ID đã cho." });
            }

            return Ok(lecturer);
        }

        [HttpGet("ByUserId/{id}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetLecturerByUserId(int id)
        {
            var lecturer = _context.Lecturers.FirstOrDefault(s => s.user_id == id);

            if (lecturer == null)
            {
                return NotFound(new { message = "Không tìm thấy giáo viên với ID đã cho." });
            }

            return Ok(lecturer);
        }

        [HttpGet("search")]
        [Authorize(Roles = "1,2,3")]
        public async Task<IActionResult> SearchLecturersByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Vui lòng nhập tên công ty cần tìm kiếm." });
            }

            var matchedLecturers = await _context.Lecturers
                .Where(l => l.lecturer_name.Contains(name))
                .ToListAsync();

            if (matchedLecturers == null || matchedLecturers.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy công ty nào phù hợp." });
            }

            return Ok(matchedLecturers);
        }

        [Authorize(Roles = "1, 3")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLecturer(int id, Lecturers lecturer)
        {
            if (id != lecturer.user_id)
            {
                return BadRequest();
            }

            _context.Entry(lecturer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LecturerExists(id))
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

        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(s => s.user_id == id);
        }
    }
}
