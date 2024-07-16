using AutoMapper;
using lms.api.Data;
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
    public class EmployeeController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IGenericRepository<Employees> _employeeRepository;
        private readonly IGenericRepository<Managers> _managersRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private string _loggedInUserId;

        public EmployeeController(
            IGenericRepository<Usermaster> userRepository,
            IGenericRepository<Employees> employeeRepository,
            ApplicationDbContext context,
            IMapper mapper
            )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _context = context;
        }

        private void GetLoggedInUserId()
        {
            _loggedInUserId = User.FindFirstValue("AiId");
        }

        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            BaseResponse<List<Employees>> response = new();
            try
            {
                var getEmployees = await _employeeRepository.GetAll();
                response.Success = true;
                response.Data = getEmployees;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetEmployeeByPagination(PaginationRequest reqModel)
        {
            PaginationResponse<IQueryable<Employees>> response = new();
            try
            {
                response = _employeeRepository.GetByPagination(reqModel, null);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("GetEmployeesByManager")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeByManager()
        {
            GetLoggedInUserId();
            BaseResponse<List<Managers>> response = new();
            try
            {
                var manager = await _userRepository.GetByCondition(x => x.AiId == Convert.ToInt64(_loggedInUserId));
                var aiId = manager.AiId;

                var employees = await _managersRepository.Find(x => x.AiId == aiId);

                response.Success = true;
                response.Data = employees;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);

        }

        [HttpGet("GetEmployeeById/{AiId:long}")]
        [Authorize]
        public async Task<IActionResult> GetEmployee([FromRoute] long AiId)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var employee = await _employeeRepository.Get(AiId);
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

        [HttpPost("CreateEmployee")]
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

                    var user = await _userRepository.GetByCondition(x => x.Email == reqModel.Email);
                    var employee = await _employeeRepository.GetByCondition(x => x.Email == reqModel.Email);

                    if (user != null || employee != null)
                    {
                        resp.Message = "Email Already Exists";
                        return Ok(resp);
                    }

                    var userEntity = _mapper.Map<Usermaster>(reqModel);
                    userEntity.AiId = await _userRepository.GenerateUniqueAiIdAsync();
                    userEntity.CreatedBy = _loggedInUserId;
                    userEntity.CreatedAt = DateTime.UtcNow;
                    userEntity.UserType = (int)UserTypes.Employee;

                    var employeeEntity = _mapper.Map<Employees>(reqModel);
                    employeeEntity.CreatedBy = _loggedInUserId;
                    employeeEntity.CreatedAt = DateTime.UtcNow;

                    await _userRepository.Create(userEntity);
                    await _employeeRepository.Create(employeeEntity);

                    resp.Success = true;
                    resp.Message = "Employee created successfully";
                    return Ok(resp);
                }
                else
                {
                    resp.Message = "Model is not valid";
                    return Ok(resp);
                }
            }
            catch (Exception ex)
            {
                resp.Message = $"An unexpected error occurred: {ex.Message}";
                return Ok(resp);
            }
        }

        [HttpPut("UpdateEmployee/{AiId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee(long AiId, [FromBody] CreateEmployeeRequest reqModel)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                if (ModelState.IsValid)
                {
                    GetLoggedInUserId();
                    var employee = await _employeeRepository.Get(AiId);
                    var employeeFromUserDb = await _userRepository.Get(AiId);

                    if (_loggedInUserId == null)
                    {
                        resp.Message = "Unable to retrieve logged-in user's ID";
                        return Ok(resp);
                    }

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


                    _mapper.Map(reqModel, employeeFromUserDb);
                    employeeFromUserDb.ModifiedBy = _loggedInUserId;
                    employeeFromUserDb.ModifiedAt = DateTime.UtcNow;

                    _mapper.Map(reqModel, employee);
                    employee.ModifiedBy = _loggedInUserId;
                    employee.ModifiedAt = DateTime.UtcNow;

                    await _userRepository.Update(employeeFromUserDb);
                    await _employeeRepository.Update(employee);

                    resp.Success = true;
                    resp.Message = "Employee updated successfully";
                    return Ok(resp);
                }
                else
                {
                    resp.Message = "Model is not valid";
                    return Ok(resp);
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
                return Ok(resp);
            }
        }

        [HttpPut("Active_Deactive/{AiId:long}")]
        [Authorize]
        public async Task<IActionResult> ChangeEmployeeStatus(long AiId, [FromQuery] bool isActive)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var userDb = await _userRepository.Get(AiId);
                var employeeDb = await _employeeRepository.Get(AiId);

                if (userDb == null || employeeDb == null)
                {
                    resp.Message = "User Not Found";
                }
                else
                {
                    userDb.Active = isActive ? 1 : 0;
                    employeeDb.Active = isActive ? 1 : 0;
                    await _userRepository.Update(userDb);
                    await _employeeRepository.Update(employeeDb);
                    resp.Success = true;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }

            return Ok(resp);
        }

        [HttpDelete("DeleteEmployee/{AiId:long}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmployee(long AiId)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var employee = await _employeeRepository.Get(AiId);
                var user = await _userRepository.Get(AiId);

                if (employee == null || user == null)
                {
                    resp.Message = "Employee Not Found";
                    return Ok(resp);
                }

                await _employeeRepository.Delete(employee);
                await _userRepository.Delete(user);
                resp.Success = true;
                resp.Message = "Manager has been Deleted";
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return Ok(resp);
        }
    }
}
