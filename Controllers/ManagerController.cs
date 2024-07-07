using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using lms.api.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IGenericRepository<Managers> _managerRepository;
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IGenericRepository<LeaveSum> _leaveSumRepository;
        private readonly IMapper _mapper;
        private string _loggedInUserId;

        public ManagerController(
            IGenericRepository<Managers> managerRepository,
            IGenericRepository<Usermaster> userRepository,
            IGenericRepository<LeaveSum> leaveSumRepository,
            IMapper mapper)
        {
            _managerRepository = managerRepository;
            _userRepository = userRepository;
            _leaveSumRepository = leaveSumRepository;
            _mapper = mapper;
        }

        private void GetLoggedInUserId()
        {
            _loggedInUserId = User.FindFirstValue("UId");
        }

        [HttpGet("GetManagerById/{ManagerId:long}")]
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

        [HttpGet("GetAllManager")]
        [Authorize]
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

        [HttpPost("AddManager")]
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

                    if (isManagerExistInManagerDB || isManagerExistUserDb)
                    {
                        response.Message = "EmployeeId Already Exists";
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

                    // Create entry in LeaveSum table
                    var leaveSumEntry = new LeaveSum
                    {
                        EmployeeId = managerMapForUserDb.EmployeeId,
                        UserType = (int)UserTypes.Employee,
                        Name = reqModel.FirstName,
                        LeavesAva = 0,
                        LeavesTaken = 0,
                        SickLeave = 10,
                        CasualLeave = 10,
                        PaidLeave = 20,
                        UnpaidLeave = 0,
                        Others = 0,
                    };
                    await _leaveSumRepository.Create(leaveSumEntry);

                    response.Success = true;
                    response.Data = managerMap;
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

        [HttpPut("UpdateManager/{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateManager([FromRoute] long ManagerId, [FromBody] CreateManagerRequest reqModel)
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

                    var managerFromUserDb = await _userRepository.GetByCondition(x => x.EmployeeId == manager.EmployeeId);
                    _mapper.Map(reqModel, managerFromUserDb);
                    managerFromUserDb.ModifiedBy = _loggedInUserId;
                    managerFromUserDb.ModifiedAt = DateTime.UtcNow;

                    _mapper.Map(reqModel, manager);
                    manager.ModifiedBy = _loggedInUserId;
                    manager.ModifiedAt = DateTime.UtcNow;

                    await _userRepository.Update(managerFromUserDb);
                    await _managerRepository.Update(manager);

                    response.Success = true;
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

        [HttpPut("Active_Deactive/{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> ChangeManagerStatus(long ManagerId, [FromQuery] bool isActive)
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

                manager.Active = isActive ? 1 : 0;
                await _managerRepository.Update(manager);
                response.Success = true;
                response.Data = manager;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpDelete("DeleteManager/{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> DeleteManager(long ManagerId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(ManagerId);
                var user = await _userRepository.GetByCondition(x => x.EmployeeId == manager.EmployeeId);

                if (manager == null || user == null)
                {
                    response.Message = "Manager Not Found";
                    return Ok(response);
                }

                await _managerRepository.Delete(manager);
                await _userRepository.Delete(user);
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
