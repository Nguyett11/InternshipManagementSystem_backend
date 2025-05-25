using IMS.DataConnection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using IMS.Models;
using IMS.Service;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionsController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public SubmissionsController(DataBaseContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        [HttpGet("ByStudentCode/{id}")]
        [Authorize(Roles = "1,2")]
        public IActionResult GetSubmissionsByStudentCode(long id)
        {
            var submissions = _context.Submissions
                .Where(s => s.student_code == id)
                .ToList();

            if (submissions == null || !submissions.Any())
            {
                return NotFound(new { message = "Không tìm thấy submission nào với studentCode đã cho." });
            }

            return Ok(submissions);
        }

        [HttpGet("ByReportId/{id}")]
        [Authorize(Roles = "1,2")]
        public IActionResult GetSubmissionsByReportId(int id)
        {
            var submissions = _context.Submissions
                .Where(s => s.report_id == id)
                .ToList();

            if (submissions == null || !submissions.Any())
            {
                return NotFound(new { message = "Không tìm thấy submission nào với ReportId đã cho." });
            }

            return Ok(submissions);
        }

        [HttpGet("ByReportIdAndStudentCode")]
        [Authorize(Roles = "1,2")]
        public IActionResult GetSubmissionByReportIdAndStudentCode([FromQuery] int reportId, [FromQuery] long studentCode)
        {
            var submissions = _context.Submissions
                .Where(s => s.report_id == reportId && s.student_code == studentCode)
                .ToList();

            if (submissions == null || !submissions.Any())
            {
                return NotFound(new { message = "Không tìm thấy submission nào với ReportId và StudentCode đã cho." });
            }

            return Ok(submissions);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetSubmissionById(int id)
        {
            var submission = _context.Submissions.FirstOrDefault(s => s.submission_id == id);

            if (submission == null)
            {
                return NotFound(new { message = "Không tìm thấy bài nộp với ID đã cho." });
            }

            var grading = _context.Grading.FirstOrDefault(g => g.submission_id == id);

            var result = new
            {
                Submission = submission,
                Grading = grading 
            };

            return Ok(result);
        }


        public class SubmissionDto
        {
            public int ReportId { get; set; }
            public long StudentCode { get; set; }
            public IFormFile File { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "1, 2")]
        public async Task<IActionResult> UploadSubmission([FromForm] SubmissionDto submission)
        {
            if (submission.File == null || submission.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file để upload." });

            // Kiểm tra phần mở rộng
            var extension = Path.GetExtension(submission.File.FileName).ToLower();
            if (extension != ".docx")
                return BadRequest(new { message = "Chỉ chấp nhận file .docx." });

            // Tạo thư mục lưu trữ nếu chưa có
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file lên server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await submission.File.CopyToAsync(stream);
            }

            // Lưu thông tin submission vào DB
            var newSubmission = new IMS.Models.Submissions
            {
                report_id = submission.ReportId,
                student_code = submission.StudentCode,
                file = uniqueFileName, // chỉ lưu tên file
                submission_date = DateTime.UtcNow,
                status = "Đã nộp"
            };

            _context.Submissions.Add(newSubmission);
            await _context.SaveChangesAsync();

            // Lấy email của giảng viên từ lecturer_id trong Reports
            var lecturerEmail = await _context.Reports
                                               .Where(r => r.report_id == submission.ReportId)
                                               .Join(_context.Lecturers,
                                                     report => report.lecturer_id,
                                                     lecturer => lecturer.lecturer_id,
                                                     (report, lecturer) => new { report, lecturer })
                                               .Join(_context.Users,
                                                     lecturer => lecturer.lecturer.user_id,
                                                     user => user.user_id,
                                                     (lecturer, user) => user.email)
                                               .FirstOrDefaultAsync();

            if (lecturerEmail == null)
            {
                return BadRequest(new { message = "Không tìm thấy email giảng viên." });
            }

            // Gửi email cho giảng viên
            var subject = "Có bài nộp mới từ sinh viên";
            var body = $"<p>Sinh viên {submission.StudentCode} vừa nộp bài cho báo cáo #{submission.ReportId}.</p>";

            await _emailService.SendEmailAsync(lecturerEmail, subject, body);

            return Ok(new { message = "Nộp bài thành công.", submission = newSubmission });
        }

    }
}
