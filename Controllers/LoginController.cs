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

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IConfiguration _configuration;
        public LoginController(IGenericRepository<Usermaster> userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<BaseResponse<AuthenticatedToken>> Login([FromBody] LoginRequest reqModel)
        {
            BaseResponse<AuthenticatedToken> resp = new BaseResponse<AuthenticatedToken>();
            try
            {
                var user = await _userRepository.Get(reqModel.EmployeeId);

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
                        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                        var token = new JwtSecurityToken(
                            issuer: _configuration["JWT:ValidIssuer"],
                            audience: _configuration["JWT:ValidAudience"],
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
