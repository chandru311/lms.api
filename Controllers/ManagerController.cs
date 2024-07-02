using AutoMapper;
using lms.api.Data;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using lms.api.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IGenericRepository<Managers> _managerRepository;
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private string _loggedInUserId;
        public ManagerController(IGenericRepository<Managers> managerRepository, IMapper mapper,
            IGenericRepository<Usermaster> userRepository)
        {
            _managerRepository = managerRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        private void GetLoggedInUserId()
        {
            _loggedInUserId = User.FindFirstValue("UId");
        }

        [HttpGet("{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> GetManagerById([FromRoute] long ManagerId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(ManagerId);
                if (manager == null)
                {
                    response.Message = "Manager Not Found";
                }
                else
                {
                    return Ok(manager);
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllManagers()
        {
            BaseResponse<Managers> response = new();
            try
            {
                var managers = await _managerRepository.GetAll();
                return Ok(managers);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateManager([FromBody] CreateManagerRequest reqModel)
        {
            GetLoggedInUserId();
            BaseResponse<Managers> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var isManagerExistInManagerDB = _managerRepository.IsRecordExists(x => x.EmployeeId == reqModel.EmployeeId);
                    var isManagerExistUserDb = _userRepository.IsRecordExists(x => x.EmployeeId == reqModel.EmployeeId);

                    if (isManagerExistInManagerDB)
                    {
                        response.Message = "EmployeeId Already Exits";
                        return Ok(response);
                    }
                    else if (isManagerExistUserDb)
                    {
                        response.Message = "EmployeeId Already Exits";
                        return Ok(response);
                    }

                    var managerMap = _mapper.Map<Managers>(reqModel);
                    managerMap.CreatedBy = _loggedInUserId;
                    managerMap.CreatedAt = DateTime.UtcNow;

                    var managerMapForUserDb = _mapper.Map<Usermaster>(reqModel);
                    managerMapForUserDb.CreatedBy = _loggedInUserId;
                    managerMapForUserDb.CreatedAt = DateTime.UtcNow;
                    managerMapForUserDb.UserType = (int)UserTypes.Manager;

                    await _userRepository.Create(managerMapForUserDb);
                    await _managerRepository.Create(managerMap);
                    response.Success = true;
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

        [HttpPut("{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateManager([FromRoute]long ManagerId, [FromBody] CreateManagerRequest reqModel)
        {
            GetLoggedInUserId();
            BaseResponse<Managers> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var manager = await _managerRepository.Get(ManagerId);

                    if (manager == null)
                    {
                        response.Message = "Manager Not Found";
                        return Ok(response);
                    }

                    var managerFromUserDb = await _userRepository.GetByCondition(x=> x.EmployeeId == manager.EmployeeId);

                    _mapper.Map(reqModel, managerFromUserDb);
                    managerFromUserDb.ModifiedBy = _loggedInUserId;
                    managerFromUserDb.ModifiedAt = DateTime.UtcNow;

                    _mapper.Map(reqModel, manager);
                    manager.ModifiedBy = _loggedInUserId;
                    manager.ModifiedAt = DateTime.Now;

                    await _userRepository.Update(managerFromUserDb);
                    await _managerRepository.Update(manager);

                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("ActivateManager{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> ActivateManager([FromRoute] long ManagerId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(ManagerId);
                if (manager == null)
                {
                    response.Message = "No Manager Found";
                    return Ok(response);
                }

                manager.Active = 1;
                await _managerRepository.Update(manager);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpDelete("DeactivateManager{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> DeactivateManager([FromRoute] long ManagerId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(ManagerId);
                if (manager == null)
                {
                    response.Message = "No Manager Found";
                    return Ok(response);
                }

                manager.Active = 0;
                await _managerRepository.Update(manager);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
