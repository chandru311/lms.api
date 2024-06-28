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
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IGenericRepository<Employees> _employeeRepository;
        private readonly IGenericRepository<Managers> _managersRepository;
        private readonly IMapper _mapper;
        private string _loggedInUserId;

        public EmployeeController(IGenericRepository<Usermaster> userRepository, IMapper mapper,
            IGenericRepository<Employees> employeeRepository, IGenericRepository<Managers> managersRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _managersRepository = managersRepository;
        }

        private void GetLoggedInUserId()
        {
            _loggedInUserId = User.FindFirstValue("UId");
        }

        [HttpGet("GetAllEmployees")]
        [Authorize]
        public async Task<IActionResult> GetAllEmployees()
        {
            var getEmployees = await _employeeRepository.GetAll();
            return Ok(getEmployees);
        }

        [HttpGet("{EmployeeId:long}")]
        [Authorize]
        public async Task<IActionResult> GetEmployee([FromRoute] long EmployeeId)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var employee = await _employeeRepository.Get(EmployeeId);
                if (employee == null)
                {
                    resp.Message = "Employee not Found";
                }
                else
                {
                    return Ok(employee);
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return Ok(resp);
        }

        [HttpPost("AddEmployee")]
        [Authorize]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest reqModel)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                if (ModelState.IsValid)
                {
                    GetLoggedInUserId();
                    if (_loggedInUserId == null)
                    {
                        resp.Message = "Unable to retrieve logged-in user's ID";
                        return Ok(resp);
                    }

                    var user = _userRepository.IsRecordExists(x => x.EmployeeId == reqModel.EmployeeId);
                    var manager = _managersRepository.IsRecordExists(x => x.ManagerId == reqModel.ManagerId);
                    if (user)
                    {
                        resp.Message = "EmployeeId Already Exists";
                        return Ok(resp);
                    }
                    else if (!manager)
                    {
                        resp.Message = "Manager Not Found";
                        return Ok(resp);
                    }

                    var userEntity = _mapper.Map<Usermaster>(reqModel);
                    userEntity.CreatedBy = _loggedInUserId;
                    userEntity.CreatedAt = DateTime.UtcNow;
                    userEntity.UserType = (int)UserTypes.Employee;

                    var employeeEntity = _mapper.Map<Employees>(reqModel);
                    employeeEntity.CreatedBy = _loggedInUserId;
                    employeeEntity.CreatedAt = DateTime.UtcNow;

                    await _employeeRepository.Create(employeeEntity);
                    await _userRepository.Create(userEntity);

                    resp.Success = true;
                }
                else
                {
                    resp.Message = "Model is not Valid";
                }

                return Ok(resp);
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return Ok(resp);
        }

        [HttpPut("UpdateEmployee/{EmployeeId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee(long EmployeeId, [FromBody] CreateEmployeeRequest reqModel)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var employee = await _employeeRepository.Get(EmployeeId);
                    var employeeFromUserDb = await _userRepository.Get(EmployeeId);
                    if (employee == null)
                    {
                        resp.Message = "No Employee Found";
                        return Ok(resp);
                    }

                    if (employeeFromUserDb == null)
                    {
                        resp.Message = "No Employee Found";
                        return Ok(resp);
                    }

                    GetLoggedInUserId();
                    if (_loggedInUserId == null)
                    {
                        resp.Message = "Unable to retrieve logged-in user's ID";
                        return Ok(resp);
                    }

                    _mapper.Map(reqModel, employeeFromUserDb);
                    employeeFromUserDb.ModifiedBy = _loggedInUserId;
                    employeeFromUserDb.ModifiedAt = DateTime.UtcNow;

                    _mapper.Map(reqModel, employee);
                    employee.CreatedBy = _loggedInUserId;
                    employee.CreatedAt = DateTime.UtcNow;

                    await _userRepository.Update(employeeFromUserDb);
                    await _employeeRepository.Update(employee);

                    resp.Success = true;
                }
                else
                {
                    resp.Message = "Model is not Valid";
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }

            return Ok(resp);
        }

        [HttpDelete("DeactivateEmployee/{EmployeeId:long}")]
        [Authorize]
        public async Task<IActionResult> DeactivateEmployee(long EmployeeId)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var userDb = await _userRepository.Get(EmployeeId);
                var employeeDb = await _employeeRepository.Get(EmployeeId);

                if (userDb == null || employeeDb == null)
                {
                    resp.Message = "User Not Found";
                }
                else
                {
                    userDb.Active = 0;
                    employeeDb.Active = 0;
                    await _userRepository.Update(userDb);
                    await _employeeRepository.Update(employeeDb);
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }

            return Ok(resp);
        }

        [HttpPut("ActivateEmployee/{EmployeeId:long}")]
        [Authorize]
        public async Task<IActionResult> ActivateEmployee(long EmployeeId)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var userDb = await _userRepository.Get(EmployeeId);
                var employeeDb = await _employeeRepository.Get(EmployeeId);

                if (userDb == null || employeeDb == null)
                {
                    resp.Message = "User Not Found";
                }
                else
                {
                    userDb.Active = 1;
                    employeeDb.Active = 1;
                    await _userRepository.Update(userDb);
                    await _employeeRepository.Update(employeeDb);
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }

            return Ok(resp);
        }
    }
}