using IMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IMS.DataConnection;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace IMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _config;

        public UsersController(DataBaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "1,2,3")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.user_id == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy sinh viên với ID đã cho." });
            }

            return Ok(user);
        }

        //Đăng nhập
        [HttpPost("login")]
        public async Task<ActionResult> DangNhap([FromBody] LoginDTO loginDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == loginDTO.email);

            if (user == null || !VerifyPassword(loginDTO.password, user.password))
            {
                return Unauthorized(new { success = false, message = "Đăng nhập thất bại" });
            }

            var claims = new[]{
            new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new Claim("id",user.user_id.ToString()),
            new Claim(ClaimTypes.Role, user.role_id.ToString())
    };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: signIn
            );
            string accesstoken = new JwtSecurityTokenHandler().WriteToken(token);

            var login_data = new
            {
                status = "ok",
                message = "Login success",
                token = accesstoken,
                user_id = user.user_id
            };

            return Ok(new { data = login_data });
        }

        public class LoginDTO
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        private bool VerifyPassword(string inputPassword, string storedHashedPassword)
        {
            string inputHashedPassword = GetSha256Hash(inputPassword);
            return StringComparer.OrdinalIgnoreCase.Compare(inputHashedPassword, storedHashedPassword) == 0;
        }

        public static string GetSha256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult> DangKy([FromForm] Users user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // 1. Kiểm tra email đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == user.email);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "Email đã tồn tại" });
            }

            // 2. Hash mật khẩu
            string hashedPassword = GetSha256Hash(user.password);

            // 3. Tạo user
            var newUser = new Users
            {
                full_name = user.full_name,
                email = user.email,
                password = hashedPassword,
                phone_number = user.phone_number,
                gender = user.gender,
                date_of_birth = user.date_of_birth,
                desired_role = user.desired_role,
                role_id = user.role_id,
                is_active = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Đăng ký thành công",
                user_id = newUser.user_id
            });
        }


        // Đăng ký
        //[HttpPost("register")]
        //public async Task<ActionResult> DangKy([FromForm] string full_name, [FromForm] string email, [FromForm] string password, [FromForm] string phone_number, [FromForm] string gender, [FromForm] DateTime date_of_birth, [FromForm] string desired_role, [FromForm] int role_id)
        //{
        //    // Kiểm tra email đã tồn tại chưa
        //    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        //    if (existingUser != null)
        //    {
        //        return BadRequest(new { success = false, message = "Email đã tồn tại" });
        //    }

        //    // Hash mật khẩu trước khi lưu
        //    string hashedPassword = GetSha256Hash(password);

        //    var newUser = new Users
        //    {
        //        full_name = full_name,
        //        email = email,
        //        password = hashedPassword,
        //        phone_number = phone_number,
        //        gender = gender,
        //        date_of_birth = date_of_birth,
        //        desired_role = desired_role,
        //        role_id = role_id,
        //        is_active = true // mặc định tài khoản mới là active
        //    };

        //    _context.Users.Add(newUser);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { success = true, message = "Đăng ký thành công", user_id = newUser.user_id });
        //}

        // API lấy chi tiết người dùng từ token
        //[Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult> GetUserProfile()
        {
            try
            {
                // Lấy claim "id" từ token
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { success = false, message = "Không tìm thấy user_id trong token" });
                }

                int userId = int.Parse(userIdClaim.Value);

                // Tìm user trong database
                var user = await _context.Users
                    //.Include(u => u.Role) // Include bảng Role để lấy thông tin role của người dùng
                    .FirstOrDefaultAsync(u => u.user_id == userId);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại" });
                }

                // Trả về thông tin user (ẩn mật khẩu)
                var userProfile = new
                {
                    user.user_id,
                    user.full_name,
                    user.email,
                    user.phone_number,
                    user.gender,
                    user.date_of_birth,
                    user.desired_role,
                    user.role_id,
                    user.is_active
                };

                return Ok(new { success = true, data = userProfile });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server", error = ex.Message });
            }
        }

        [Authorize(Roles = "1, 2,3")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, Users user)
        {
            if (id != user.user_id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.user_id == id);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful" });
        }

    }
}



//Đăng nhập
//[HttpPost("login")]
//public async Task<ActionResult> DangNhap([FromForm] string username, [FromForm] string password)
//{
//    // Debugging: Log the received username and password
//    Console.WriteLine($"Username: {username}, Password: {password}");

//    var user = await _context.Users
//        .FirstOrDefaultAsync(u => u.email == username);

//    if (user == null || !VerifyPassword(user, password))
//    {
//        return Unauthorized(new { success = false, message = "Đăng nhập thất bại" });

//    }

//    //create token
//    var claims = new[]{
//            new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
//            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
//            new Claim("id",user.user_id.ToString()),
//            new Claim(ClaimTypes.Role, user.role_id.ToString())

//     };
//    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
//    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//    var token = new JwtSecurityToken(
//        _config["Jwt:Issuer"],
//        _config["Jwt:Audience"],
//        claims,
//        expires: DateTime.UtcNow.AddMinutes(60),
//        signingCredentials: signIn
//        );
//    string accesstoken = new JwtSecurityTokenHandler().WriteToken(token);

//    var login_data = new
//    {
//        status = "ok",
//        message = "Login success",
//        token = accesstoken,
//        user = user.full_name
//    };

//    return Ok(new { data = login_data });
//}