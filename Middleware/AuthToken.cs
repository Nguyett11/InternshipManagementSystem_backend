using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IMS.Middleware
{
    public class AuthToken
    {

        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public AuthToken(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token != null)
                {
                    //Console.WriteLine(token);
                    var auth = AttachUserToContext(context, token);
                    if (auth)
                    {
                        await _next(context);
                    }
                    else
                    {
                        var myObject = new { status = "no", message = "token is incorrect" };
                        var jsonResponse = JsonConvert.SerializeObject(myObject);
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync(jsonResponse);
                    }
                }
                else
                {
                    var myObject = new { status = "no", message = "Unauthorized: Missing or invalid token." };
                    var jsonResponse = JsonConvert.SerializeObject(myObject);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(jsonResponse);
                }

            }
            catch (Exception ex)
            {
                var myObject = new { status = "no", message = ex.Message };
                var jsonResponse = JsonConvert.SerializeObject(myObject);
                await context.Response.WriteAsync(jsonResponse);

            }
        }

        private Boolean AttachUserToContext(HttpContext context, string token)
        {
            // Thực hiện giải mã token và lưu thông tin vào HttpContext
            // Ví dụ sử dụng JWT để giải mã token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            try
            {
                var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // Đặt ClockSkew bằng TimeSpan.Zero để bỏ qua khoảng thời gian cho phép
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var IdUser = jwtToken.Claims.First(x => x.Type == "IdUser").Value;
                var UserName = jwtToken.Claims.First(x => x.Type == "UserName").Value;


                // Đính kèm thông tin người dùng vào context
                context.Items["IdUser"] = IdUser;
                context.Items["UserName"] = UserName;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}