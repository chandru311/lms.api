using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace lms.api.Services
{
    public class ForgotPasswordService
    {
        private readonly IConfiguration _configuration;
        public ForgotPasswordService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateForgotPasswordToken(long uid, string email)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Email, email),
                new("uid", uid.ToString())
            };

            var secret = _configuration["JWT:Secret"];
            var issuer = _configuration["JWT:ValidIssuer"];
            var audience = _configuration["JWT:ValidAudience"];

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires:  DateTime.UtcNow.AddDays(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey,SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public async Task SendForgotPassLink(string to, string name, long uid, string referer)
        {
            var resetToken = GenerateForgotPasswordToken(uid, to);
            var link = $"{referer}reset-password/{resetToken}";
            var subject = "Reset Password";
            var body = $"Hey {name} , Below link is your link to reset the current password : {link}";

            return; 
        }
    }
}
