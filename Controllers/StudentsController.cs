using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public StudentsController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("ByStudentCode/{id}")]
        [Authorize(Roles = "1,2, 3")]
        public IActionResult GetStudentByStudentCode(long id)
        {
            var student = _context.Students.FirstOrDefault(s => s.student_code == id);

            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên với mã code đã cho." });
            }

            return Ok(student);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2, 3")]
        public IActionResult GetStudentByUserId(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.user_id == id);

            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên với ID đã cho." });
            }

            return Ok(student);
        }

        [HttpGet("ByLecturerId/{id}")]
        [Authorize(Roles = "1,3")]
        public IActionResult GetByLecturerId(int id)
        {
            var students = _context.Students.Where(s => s.lecturer_id == id).ToList();

            if (students == null || students.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên nào với ID giảng viên đã cho." });
            }

            return Ok(students);
        }


        [Authorize(Roles = "1, 2")]
        [HttpPost]
        public async Task<IActionResult> PostStudent([FromBody] Students student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra trùng ID (nếu cần)
            if (_context.Students.Any(s => s.user_id == student.user_id))
            {
                return Conflict(new { message = "Sinh viên với user_id này đã tồn tại." });
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentByUserId), new { id = student.user_id }, student);
        }

        [HttpGet("search")]
        [Authorize(Roles = "1,2,3")]
        public async Task<IActionResult> SearchStudentsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Vui lòng nhập tên sinh viên cần tìm kiếm." });
            }

            var matchedStudents = await _context.Students
             .Where(s => s.student_name.Contains(name))
             .ToListAsync();


            if (matchedStudents == null || matchedStudents.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên nào phù hợp." });
            }

            return Ok(matchedStudents);
        }

        [Authorize(Roles = "1, 2")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Students students)
        {
            if (id != students.user_id)
            {
                return BadRequest();
            }

            _context.Entry(students).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        private bool StudentExists(int id)
        {
            return _context.Students.Any(s => s.user_id == id);
        }
    }
}
