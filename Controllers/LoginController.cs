using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using lms.api.Data;
using Microsoft.EntityFrameworkCore;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        public LoginController(IGenericRepository<Usermaster> userRepository, IConfiguration configuration,
            ApplicationDbContext context
        )
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _context = context;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<BaseResponse<AuthenticatedToken>> Login([FromBody] LoginRequest reqModel)
        {
            BaseResponse<AuthenticatedToken> resp = new BaseResponse<AuthenticatedToken>();
            try
            {
                var user = await _context.Usermasters.FirstOrDefaultAsync(x => x.EmployeeId == reqModel.EmployeeId);

                if (user == null)
                {
                    resp.Message = "Invalid Credentials";
                    return resp;
                }
                else if(user.Active == 0)
                {
                    resp.Message = "User is Inactive";
                    return resp;
                }
                else
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim("UId", user.UId.ToString()),
                        new Claim("EmployeeId", user.EmployeeId.ToString()),
                        new Claim("UserType", user.UserType.ToString()),
                    };

                    var validUserEmployeeId = user.EmployeeId == reqModel.EmployeeId;
                    var validUserPassword = user.Password == reqModel.Password;

                    if (validUserEmployeeId && validUserPassword)
                    {
                        var jwtSecret = _configuration["JWT:Secret"];
                        var jwtIssuer = _configuration["JWT:ValidIssuer"];
                        var jwtAudience = _configuration["JWT:ValidAudience"];

                        if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                        {
                            resp.Message = "JWT configuration values are missing.";
                            return resp;
                        }

                        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

                        var token = new JwtSecurityToken(
                            issuer: jwtIssuer,
                            audience: jwtAudience,
                            expires: DateTime.UtcNow.AddDays(1),
                            claims: authClaims,
                            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                        resp.Data = new AuthenticatedToken()
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            Expiration = token.ValidTo,
                            EmployeeId = user.EmployeeId,
                            UserType  = user.UserType,

                        };
                        resp.Success = true;
                        resp.Message = "Login Successful";
                    }
                    else
                    {
                        resp.Message = "Invalid Credentials";
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return resp;
        }
    }
}
