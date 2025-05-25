using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static IMS.Controllers.ReportsController;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsStudentController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public ReportsStudentController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("ById/{id}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetReportStudentById(int id)
        {
            var report = _context.ReportStudents.FirstOrDefault(r => r.report_student_id == id);

            if (report == null)
            {
                return NotFound(new { message = "Không tìm thấy report với ID đã cho." });
            }

            return Ok(report);
        }

        [HttpPost]
        [Authorize(Roles = "1,3")]
        public async Task<ActionResult<Reports>> PostReportStudent(ReportStudents reportStudent)
        {
            _context.ReportStudents.Add(reportStudent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReportStudentById), new { id = reportStudent.report_student_id }, reportStudent);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2")]
        public IActionResult GetReportsByStudentCode(long id)
        {
            var result = (from r in _context.Reports
                          join rs in _context.ReportStudents on r.report_id equals rs.report_id
                          join s in _context.Submissions on new { r.report_id, student_code = id } equals new { s.report_id, s.student_code } into sj
                          from submission in sj.DefaultIfEmpty()
                          join g in _context.Grading on submission.submission_id equals g.submission_id into gj
                          from grading in gj.DefaultIfEmpty()
                          where rs.student_code == id
                          select new ReportWithGradingAndSubmissionDto
                          {
                              Report = r,
                              Grading = grading,
                              Submission = submission
                          }).ToList();

            if (!result.Any())
            {
                return NotFound("Không tìm thấy báo cáo nào cho sinh viên này.");
            }

            return Ok(result);
        }


        [HttpGet("search")]
        [Authorize(Roles = "1,2")]
        public IActionResult SearchReports(long studentCode, string searchTerm = null)
        {
            var query = (from r in _context.Reports
                         join rs in _context.ReportStudents on r.report_id equals rs.report_id
                         join s in _context.Submissions on new { r.report_id, student_code = studentCode } equals new { s.report_id, s.student_code } into sj
                         from submission in sj.DefaultIfEmpty()
                         join g in _context.Grading on submission.submission_id equals g.submission_id into gj
                         from grading in gj.DefaultIfEmpty()
                         where rs.student_code == studentCode
                         select new ReportWithGradingAndSubmissionDto
                         {
                             Report = r,
                             Grading = grading,
                             Submission = submission
                         });

            // Thêm tìm kiếm theo tên báo cáo hoặc thông tin khác
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(report => report.Report.title.Contains(searchTerm) ||
                                               (report.Submission != null && report.Submission.status.Contains(searchTerm)) ||
                                               (report.Grading != null && report.Grading.grade.Contains(searchTerm)));
            }

            var result = query.ToList();

            if (!result.Any())
            {
                return NotFound("Không tìm thấy báo cáo nào với từ khóa tìm kiếm này.");
            }

            return Ok(result);
        }

        public class StudentWithSubmissionDto
        {
            public Students Student { get; set; }
            public Submissions Submission { get; set; }
            public Grading? Grading { get; set; }
        }

        [HttpGet("report-students/{reportId}")]
        [Authorize(Roles = "1,3")]
        public IActionResult GetStudentsByReportId(int reportId)
        {
            var result = (from rs in _context.ReportStudents
                          join s in _context.Students on rs.student_code equals s.student_code
                          join sub in _context.Submissions
                              on new { rs.student_code, rs.report_id } equals new { sub.student_code, sub.report_id }
                              into subJoin
                          from submission in subJoin.DefaultIfEmpty()

                              // Join grading theo submission_id (nếu có submission)
                          join g in _context.Grading on submission.submission_id equals g.submission_id into gradingJoin
                          from grading in gradingJoin.DefaultIfEmpty()

                          where rs.report_id == reportId
                          select new StudentWithSubmissionDto
                          {
                              Student = s,
                              Submission = submission,
                              Grading = grading
                          }).ToList();

            if (!result.Any())
            {
                return NotFound("Không có sinh viên nào cho báo cáo này.");
            }

            return Ok(result);
        }

        [HttpGet("report-students/search")]
        [Authorize(Roles = "1,3")]
        public IActionResult SearchStudentsByReportId(int reportId, string searchTerm)
        {
            searchTerm = searchTerm?.ToLower().Trim();

            var result = (from rs in _context.ReportStudents
                          join s in _context.Students on rs.student_code equals s.student_code
                          join sub in _context.Submissions
                              on new { rs.student_code, rs.report_id } equals new { sub.student_code, sub.report_id }
                              into subJoin
                          from submission in subJoin.DefaultIfEmpty()

                          join g in _context.Grading on submission.submission_id equals g.submission_id into gradingJoin
                          from grading in gradingJoin.DefaultIfEmpty()

                          where rs.report_id == reportId &&
                                (string.IsNullOrEmpty(searchTerm) ||
                                 s.student_name.ToLower().Contains(searchTerm) ||
                                 s.student_code.ToString().Contains(searchTerm) ||
                                 s.class_student.ToLower().Contains(searchTerm))

                          select new StudentWithSubmissionDto
                          {
                              Student = s,
                              Submission = submission,
                              Grading = grading
                          }).ToList();

            if (!result.Any())
            {
                return NotFound("Không tìm thấy sinh viên nào phù hợp.");
            }

            return Ok(result);
        }

        public class ReportWithGradingAndSubmissionDto
        {
            public Reports Report { get; set; }
            public Grading Grading { get; set; }
            public Submissions Submission { get; set; }
        }

        private object await(IQueryable<Reports> reports)
        {
            throw new NotImplementedException();
        }
    }
}
