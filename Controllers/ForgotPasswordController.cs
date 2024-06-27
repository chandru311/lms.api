using AutoMapper;
using lms.api.Data;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using lms.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly ForgotPasswordService _forgotPasswordService;
        private readonly IMapper _mapper;
        public ForgotPasswordController(IGenericRepository<Usermaster> userRepository, ApplicationDbContext context,
            ForgotPasswordService forgotPasswordService, IMapper mapper)
        {
            _userRepository = userRepository;
            _context = context;
            _forgotPasswordService = forgotPasswordService;
            _mapper = mapper;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordRequest reqModel)
        {
            BaseResponse<UsermasterResponse> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _context.Usermasters.FirstOrDefaultAsync(x => x.Email == reqModel.Email);
                    if (user == null)
                    {
                        response.Message = "Email does not exist";
                    }

                    var emailLink = Request.Headers.Referer;
                    await _forgotPasswordService.SendForgotPassLink(reqModel.Email, reqModel.Email, user.EmployeeId, emailLink);
                    var userMasterMap = _mapper.Map<Usermaster, UsermasterResponse>(user);
                    response.Success = true;
                    response.Data = userMasterMap;
                    response.Message = "Reset Link is sent to your registered email";
                }
                else
                {
                    response.Message = "Model is not valid";
                }
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
