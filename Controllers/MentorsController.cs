using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MentorsController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public MentorsController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<IEnumerable<Mentors>>> GetMentors()
        {
            return await _context.Mentors.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetMentorById(int id)
        {
            var mentor = _context.Mentors.FirstOrDefault(s => s.mentor_id == id);

            if (mentor == null)
            {
                return NotFound(new { message = "Không tìm thấy mentor với ID đã cho." });
            }

            return Ok(mentor);
        }

        [HttpGet("search")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> SearchMentorsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Vui lòng nhập tên công ty cần tìm kiếm." });
            }

            var matchedMentors = await _context.Mentors
                .Where(m => m.mentor_name.Contains(name))
                .ToListAsync();

            if (matchedMentors == null || matchedMentors.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy công ty nào phù hợp." });
            }

            return Ok(matchedMentors);
        }
    }
}
