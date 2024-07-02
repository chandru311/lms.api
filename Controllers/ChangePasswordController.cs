using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        public ChangePasswordController(IGenericRepository<Usermaster> userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordRequest reqModel)
        {
            BaseResponse<UsermasterResponse> response = new BaseResponse<UsermasterResponse>();
            try
            {
                if(ModelState.IsValid)
                {
                    var userClaims = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UID")?.Value);
                    var user = await _userRepository.Get(userClaims);
                    if(user == null)
                    {
                        response.Message = "Not a valid user or not configured properly";
                        return Ok(response);
                    }
                    else
                    {
                        if(user.Password == reqModel.OldPassword)
                        {
                            user.Password = reqModel.NewPassword;
                            await _userRepository.Update(user);

                            response.Success = true;
                            response.Message = "Password has Changed";
                        }
                        else
                        {
                            response.Success = false;
                            response.Message = "Password does not match, Please try again";
                            return Ok(response);
                        }
                    }
                }
                else
                {
                    response.Message = "Model is not Valid";
                }
                return Ok(response);

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
