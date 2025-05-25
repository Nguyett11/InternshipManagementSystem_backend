using IMS.DataConnection;
using IMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public CompaniesController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult<IEnumerable<Companies>>> GetCompanies()
        {
            return await _context.Companies.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2, 3")]
        public IActionResult GetCompanyById(int id)
        {
            var company = _context.Companies.FirstOrDefault(s => s.company_id == id);

            if (company == null)
            {
                return NotFound(new { message = "Không tìm thấy công ty với ID đã cho." });
            }

            return Ok(company);
        }

        [HttpGet("search")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> SearchCompaniesByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Vui lòng nhập tên công ty cần tìm kiếm." });
            }

            var matchedCompanies = await _context.Companies
                .Where(c => c.company_name.Contains(name))
                .ToListAsync();

            if (matchedCompanies == null || matchedCompanies.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy công ty nào phù hợp." });
            }

            return Ok(matchedCompanies);
        }

    }
}
