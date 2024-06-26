using AutoMapper;
using lms.api.Data;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using lms.api.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public EmployeeController(IGenericRepository<Usermaster> userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
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
        public async Task<IActionResult> GetEmployee([FromRoute]long EmployeeId)
        {
            BaseResponse<UsermasterResponse> resp = new();
            try
            {
                var employee = await _employeeRepository.Get(EmployeeId);
                if (employee == null)
                {
                    resp.Message = "Employee not Found";
                    resp.Success = false;
                    return Ok(resp);
                }
                else
                {
                    return Ok(employee);
                }
            }catch (Exception ex)
            {
                resp.Message = ex.Message;
            }
            return Ok(resp);
        }

        [HttpPost("AddEmployee")]
        [Authorize]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest reqModel)
        {
            BaseResponse<UsermasterResponse> resp = new();
            try
            {
                if (ModelState.IsValid)
                {

                    var user = _userRepository.IsRecordExists(x => x.EmployeeId == reqModel.EmployeeId);
                    if (user)
                    {
                        resp.Success = false;
                        resp.Message = "EmployeeId Already Exists";
                        return Ok(resp);
                    }

                    var loggedInUserId = User.FindFirstValue("UId");
                    if (loggedInUserId == null)
                    {
                        resp.Success = false;
                        resp.Message = "Unable to retrieve logged-in user's ID";
                        return Ok(resp);
                    }

                    var emp = _mapper.Map<Employees>(reqModel);
                    emp.ManagerId = Convert.ToInt64(loggedInUserId);
                    emp.Active = 1;
                    emp.CreatedAt = DateTime.Now;
                    emp.CreatedBy = reqModel.EmployeeId.ToString();


                    var userDb = new Usermaster()
                    {
                        EmployeeId = reqModel.EmployeeId,
                        MobileNumber = reqModel.MobileNumber,
                        Password = reqModel.Password,
                        UserType = (int)UserTypes.Employee,
                        Active = 1,
                        CreatedAt = DateTime.Now,
                        CreatedBy = reqModel.EmployeeId.ToString(),
                    };

                    await _employeeRepository.Create(emp);
                    await _userRepository.Create(userDb);

                    resp.Success = true;
                    return Ok(resp);
                }
                else
                {
                    resp.Success = false;
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

        [HttpPut("UpdateEmployee")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee([FromRoute]long EmployeeId, [FromBody] UpdateEmployeeRequest reqModel)
        {
            BaseResponse<UsermasterResponse> resp = new();
            try
            {
                if(ModelState.IsValid)
                {
                    var employee = await _employeeRepository.Get(EmployeeId);
                    var employeeFromUserDb = await _userRepository.Get(EmployeeId);
                    if(employee == null)
                    {
                        resp.Success = false;
                        resp.Message = "No Employee Found";
                    }

                    if(employeeFromUserDb == null)
                    {
                        resp.Success = false;
                        resp.Message = "No Employee Found";
                    }

                    var mapEmployee = _mapper.Map<Employees>(reqModel);
                    var mapEmployeeFromUserDb = _mapper.Map<Usermaster>(reqModel);

                    //employee.Email = reqModel.Email,
                    //employee.State = reqModel.State,
                    //employee.Country = reqModel.Country,

                    //employeeFromUserDb.UserType = reqModel.UserType,

                    await _employeeRepository.Update(mapEmployee);
                    await _userRepository.Update(mapEmployeeFromUserDb);


                    resp.Success = true;
                    resp.Message = "Employee Updated";

                }
                else
                {
                    resp.Success = false;
                    resp.Message = "Model is not Valid";
                }
            }
            catch(Exception ex)
            {
                resp.Message=ex.Message;
            }

            return Ok(resp);
        }

        [HttpDelete("DeactivateEmployee")]
        [Authorize]
        public async Task<IActionResult> DeactivateEmployee([FromRoute]long EmployeeId)
        {
            BaseResponse<UsermasterResponse> resp = new();
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

        [HttpPut("ActivateEmployee")]
        [Authorize]
        public async Task<IActionResult> ActivateEmployee([FromRoute] long EmployeeId)
        {
            BaseResponse<UsermasterResponse> resp = new();
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
