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
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IGenericRepository<Employees> _employeeRepository;
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
        [Authorize]
        public async Task<IActionResult> GetAllEmployees()
        {
            BaseResponse<List<Employees>> response = new();

            var getEmployees = await _employeeRepository.GetAll();
            response.Success = true;
            response.Data = getEmployees;
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

        [HttpGet("GetEmployeesByManagerId")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeByManagerId()
        {
            GetLoggedInUserId();
            BaseResponse<List<Managers>> response = new();
            try
            {
                var manager = await _userRepository.GetByCondition(x => x.UId == Convert.ToInt64(_loggedInUserId));
                var employeeId = manager.EmployeeId;

                var employees = await _managersRepository.Find(x => x.EmployeeId == employeeId);
                
                response.Success = true;
                response.Data = employees;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
            
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

                    var user = await _userRepository.GetByCondition(x => x.Email == reqModel.Email);
                    if (user != null)
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

                    await _employeeRepository.Create(employeeEntity);
                    await _userRepository.Create(userEntity);

                    var leaveSumEntity = new LeaveSum
                    {
                        AiId = employeeEntity.AiId,
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

                    _context.LeaveSums.Add(leaveSumEntity);
                    await _context.SaveChangesAsync();

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
                resp.Message = ex.Message;
                return Ok(resp);
            }
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


        [HttpPut("Active_Deactive/{EmployeeId:long}")]
        [Authorize]
        public async Task<IActionResult> ChangeEmployeeStatus(long EmployeeId, [FromQuery] bool isActive)
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

        [HttpDelete("DeleteEmployee/{EmployeeId:long}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmployee(long EmployeeId)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var employee = await _employeeRepository.Get(EmployeeId);
                var user = await _userRepository.Get(EmployeeId);

                if (employee == null || user == null)
                {
                    resp.Message = "Employee Not Found";
                    return Ok(resp);
                }

                await _employeeRepository.Delete(employee);
                await _userRepository.Delete(user);
                resp.Success = true;
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return Ok(resp);
        }
    }
}
