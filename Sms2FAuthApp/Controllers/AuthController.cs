using Dapper;
using Microsoft.AspNetCore.Mvc;
using Sms2FAuthApp.Models;
using Sms2FAuthApp.Services;
using System.Data.SqlClient;

namespace Sms2FAuthApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly PasswordService _passwordService;
        private readonly SmsService _smsService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration config, PasswordService PasswordService, SmsService smsService, ILogger<AuthController> logger)
        {
            _config = config;
            _passwordService = PasswordService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                      "SELECT Id FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var id = connection.QuerySingleOrDefault<string>(sql, new { Username = model.Username, PasswordHash = hashedPass });

                if (id != null)
                {
                    return Ok(id);
                }
                else
                {
                    return BadRequest();
                }
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterModel model)
        {
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value};" + "INSERT INTO Users (Username, PasswordHash, PhoneNumber) VALUES (@Username, @PasswordHash, @PhoneNumber)";

            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                try
                {
                    var sqlUserExists = $"USE {_config.GetRequiredSection("DB:Name").Value};" + "SELECT * FROM Users WHERE Username = @Username";
                    var existingUser = connection.QuerySingleOrDefault(sqlUserExists, new { model.Username });

                    if (existingUser != null)
                    {
                        return BadRequest("Username already exists.");
                    }

                    connection.Execute(sql, new { Username = model.Username, PasswordHash = hashedPass, PhoneNumber = model.PhoneNumber });

                    return Ok("User registered successfully.");
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpGet("send-verification")]
        public IActionResult SendVerificationCode([FromQuery] string userId)
        {
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                      "SELECT PhoneNumber FROM Users WHERE Id = @Id";
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var phoneNumber = connection.QuerySingleOrDefault<string>(sql, new { Id = int.TryParse(userId, out var parsedUserId) });
                if (phoneNumber != null)
                {
                    var response = _smsService.SendVerification(phoneNumber);

                    if (response.Status == TaskStatus.WaitingForActivation || response.Status == TaskStatus.Created)
                    {
                        return Ok(response.Result.RequestId);
                    }
                    else
                    {
                        return BadRequest($"Verification failed: {response.Exception?.Message}");
                    }
                }
                return BadRequest($"Verification failed: User not found.");
            }

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyUser([FromBody] SmsVerificationModel data)
        {
            var response = await _smsService.CheckVerification(data.RequestId, data.Code);
            if (response.Status == "0")
            {
                return Ok("User verified successfully!");
            }
            else
            {
                return BadRequest($"Verification failed: {response.ErrorText}");
            }
        }
    }
}
