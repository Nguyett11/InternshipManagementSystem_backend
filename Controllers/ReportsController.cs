using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Composition;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public ReportsController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<IEnumerable<Reports>>> GetReports()
        {
            return await _context.Reports.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetReportById(int id)
        {
            var report = _context.Reports.FirstOrDefault(r => r.report_id == id);

            if (report == null)
            {
                return NotFound(new { message = "Không tìm thấy report với ID đã cho." });
            }

            var grading = _context.Grading.FirstOrDefault(g => g.submission_id == id);

            var result = new ReportWithGradingDto
            {
                Report = report,
                Grading = grading // có thể null nếu chưa có grading
            };

            return Ok(result);
        }

        public class ReportWithGradingDto
        {
            public Reports Report { get; set; }
            public Grading? Grading { get; set; } // Có thể null nếu chưa chấm điểm
        }

        [HttpGet("ByUnsubmitted")]
        [Authorize(Roles = "1,2")]
        public IActionResult GetUnsubmittedReports([FromQuery] long studentCode)
        {
            var unsubmittedReports = (from rs in _context.ReportStudents
                                      join r in _context.Reports on rs.report_id equals r.report_id
                                      where rs.student_code == studentCode &&
                                            !_context.Submissions.Any(sub =>
                                                sub.report_id == rs.report_id &&
                                                sub.student_code == studentCode &&
                                                sub.status == "Đã nộp")
                                      select r).ToList();

            return Ok(unsubmittedReports);
        }


        [HttpGet("SearchUnsubmittedReports")]
        [Authorize(Roles = "1,2")]
        public IActionResult SearchUnsubmittedReports([FromQuery] long studentCode, [FromQuery] string? searchTerm)
        {
            var unsubmittedReports = (from rs in _context.ReportStudents
                                      join r in _context.Reports on rs.report_id equals r.report_id
                                      where rs.student_code == studentCode &&
                                            !_context.Submissions.Any(sub =>
                                                sub.report_id == rs.report_id &&
                                                sub.student_code == studentCode &&
                                                sub.status == "Đã nộp") &&
                                            (string.IsNullOrEmpty(searchTerm) || r.title.Contains(searchTerm))
                                      select r).ToList();

            if (unsubmittedReports.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy báo cáo chưa nộp nào theo yêu cầu." });
            }

            return Ok(unsubmittedReports);
        }


        [HttpGet("ByLecturer/{lecturerId}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetReportsByLecturerId(int lecturerId)
        {
            var reports = _context.Reports
                .Where(r => r.lecturer_id == lecturerId)
                .ToList();

            if (reports == null || reports.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy báo cáo nào của giảng viên với ID đã cho." });
            }

            return Ok(reports);
        }

        [HttpGet("SearchByLecturer")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult SearchReportsByLecturer([FromQuery] int lecturerId, [FromQuery] string? searchTerm)
        {
            var query = _context.Reports
                .Where(r => r.lecturer_id == lecturerId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.title.Contains(searchTerm));
            }

            var result = query.ToList();

            if (result.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy báo cáo nào phù hợp." });
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "1,3")]
        public async Task<ActionResult<Reports>> PostReport(Reports report)
        {
            if (string.IsNullOrWhiteSpace(report.title))
            {
                return BadRequest(new { message = "Tiêu đề báo cáo không được để trống." });
            }

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReportById), new { id = report.report_id }, report);
        }


        [Authorize(Roles = "1, 3")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, Reports report)
        {
            if (id != report.report_id)
            {
                return BadRequest();
            }

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
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

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(r => r.report_id == id);
        }
    }
}
