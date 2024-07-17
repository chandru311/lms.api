using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using lms.api.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IGenericRepository<Managers> _managerRepository;
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IMapper _mapper;
        private string _loggedInUserId;

        public ManagerController(
            IGenericRepository<Managers> managerRepository,
            IGenericRepository<Usermaster> userRepository,
            IMapper mapper)
        {
            _managerRepository = managerRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private void GetLoggedInUserId()
        {
            /* _loggedInUserId = User.FindFirstValue("AiId");*/
            _loggedInUserId = "1";
        }

        [HttpGet("GetAllManagers")]
        public async Task<IActionResult> GetAllManagers()
        {
            BaseResponse<IEnumerable<Managers>> response = new();
            try
            {
                var managers = await _managerRepository.GetAll();
                response.Success = true;
                response.Data = managers;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("GetManagerByPagination")]
        public IActionResult GetManagerByPagination(PaginationRequest reqModel)
        {
            PaginationResponse<IQueryable<Managers>> response = new();
            try
            {
                response = _managerRepository.GetByPagination(reqModel, null);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("GetManagerById/{AiID:long}")]
        public async Task<IActionResult> GetManagerById([FromRoute] long AiId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(AiId);
                if (manager == null)
                {
                    response.Message = "Manager Not Found";
                }
                else
                {
                    response.Success = true;
                    response.Data = manager;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost("CreateManager")]
        public async Task<IActionResult> CreateManager([FromBody] CreateManagerRequest reqModel)
        {
            BaseResponse<Managers> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    /* GetLoggedInUserId();
                     if (_loggedInUserId == null)
                     {
                         response.Message = "Unable to retrieve logged-in user's ID";
                         return Ok(response);
                     }*/

                    var user = await _userRepository.GetByCondition(x => x.Email == reqModel.Email);
                    var manager = await _managerRepository.GetByCondition(x => x.Email == reqModel.Email);

                    if (manager != null && user != null)
                    {
                        response.Message = "Email Already Exists";
                        return Ok(response);
                    }

                    var userEntity = _mapper.Map<Usermaster>(reqModel);
                    userEntity.AiId = await _userRepository.GenerateUniqueAiIdAsync();
                    userEntity.CreatedBy = "Admin";
                    userEntity.CreatedAt = DateTime.UtcNow;
                    userEntity.UserType = (int)UserTypes.Manager;

                    var managerEntity = _mapper.Map<Managers>(reqModel);
                    managerEntity.AiId = userEntity.AiId;
                    managerEntity.CreatedBy = "Admin";
                    managerEntity.CreatedAt = DateTime.UtcNow;


                    await _userRepository.Create(userEntity);
                    await _managerRepository.Create(managerEntity);

                    response.Success = true;
                    response.Message = "Manager created successfully";
                    response.Data = managerEntity;
                }
                else
                {
                    response.Message = "Model is not valid";
                }
            }
            catch (DbUpdateException dbEx)
            {
                response.Message = $"An error occurred while saving the entity changes. See the inner exception for details: {dbEx.InnerException?.Message}";
            }
            catch (Exception ex)
            {
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }
            return Ok(response);
        }

        [HttpPut("UpdateManager/{AiId:long}")]
        public async Task<IActionResult> UpdateManager([FromRoute] long AiId, [FromBody] CreateManagerRequest reqModel)
        {
            BaseResponse<Managers> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    GetLoggedInUserId();
                    var manager = await _managerRepository.Get(AiId);
                    var managerFromUserDb = await _userRepository.Get(AiId);

                    if (_loggedInUserId == null)
                    {
                        response.Message = "Unable to retrieve logged-in user's ID";
                        return Ok(response);
                    }

                    if (manager == null)
                    {
                        response.Message = "Manager Not Found";
                        return Ok(response);
                    }

                    if (managerFromUserDb == null)
                    {
                        response.Message = "No Manager Found";
                        return Ok(response);
                    }

                    _mapper.Map(reqModel, managerFromUserDb);
                    managerFromUserDb.ModifiedBy = _loggedInUserId;
                    managerFromUserDb.ModifiedAt = DateTime.UtcNow;

                    _mapper.Map(reqModel, manager);
                    manager.ModifiedBy = _loggedInUserId;
                    manager.ModifiedAt = DateTime.UtcNow;

                    await _userRepository.Update(managerFromUserDb);
                    await _managerRepository.Update(manager);

                    response.Success = true;
                    response.Message = "Manager updated successfully";
                    response.Data = manager;
                }
                else
                {
                    response.Message = "Model is not valid";
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("Active_Deactive/{AiId:long}")]
        public async Task<IActionResult> ChangeManagerStatus(long AiId, [FromQuery] bool isActive)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var userDb = await _userRepository.Get(AiId);
                var managerDb = await _managerRepository.Get(AiId);
                if (userDb == null && managerDb == null)
                {
                    response.Message = "User Not Found";
                }
                else
                {
                    userDb.Active = isActive ? 1 : 0;
                    managerDb.Active = isActive ? 1 : 0;
                    await _userRepository.Update(userDb);
                    await _managerRepository.Update(managerDb);
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpDelete("DeleteManager/{AiId:long}")]
        public async Task<IActionResult> DeleteManager(long AiId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(AiId);
                var user = await _userRepository.Get(AiId);

                if (manager == null && user == null)
                {
                    response.Message = "Manager Not Found";
                    return Ok(response);
                }

                await _managerRepository.Delete(manager);
                await _userRepository.Delete(user);
                response.Success = true;
                response.Message = "Manager has been Deleted";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
