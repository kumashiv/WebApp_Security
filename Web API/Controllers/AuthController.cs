using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Web_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public AuthController(IConfiguration configuration)     // Dependency inject Iconfiguration for secret key in appsettings
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody]Credential credential)      // Receive credential from Body 
        {
            // Verify the credential
            if (credential.UserName == "admin" && credential.Password == "123")
            {
                // creating the security context
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Email, "admin@mywebsite.com"),
                    new Claim("Department", "HR"),
                    new Claim("Admin", "true"),
                    new Claim("Manager", "true"),
                    new Claim("EmploymentDate", "2024-05-01")
                };

                var expiresAt = DateTime.UtcNow.AddMinutes(10);

                return Ok(new
                {
                    access_token = CreateToken(claims, expiresAt),      // Get token here when created
                    expires_at = expiresAt
                });
            }

            // If credential is incorrect
            ModelState.AddModelError("Unauthorized", "You're not authorized to access the endpoint.");
            return Unauthorized(ModelState);
        }

        private string CreateToken(IEnumerable<Claim> claims, DateTime expireAt)
        {
            var secretkey = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretKey")??"");
            // Generate the JWT
            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expireAt,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretkey),                         // This is our key we have in diagram to decode claims etc.
                    SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }

    public class Credential
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
