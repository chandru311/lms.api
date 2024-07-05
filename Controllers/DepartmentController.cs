using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Managers> _managerRepository;
        private readonly IGenericRepository<Employees> _employeeRepository;
        private readonly IMapper _mapper;

        public DepartmentsController(
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Managers> managerRepository,
            IGenericRepository<Employees> employeeRepository,
            IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _managerRepository = managerRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        private async Task PopulateDepartmentDetails(Departments department)
        {
            if (department.ManagerId.HasValue)
            {
                var manager = await _managerRepository.Get(department.ManagerId.Value);
                department.DepartmentHead = manager?.FirstName;
            }

            // Count the employees in the department
            var employeesCount = await _employeeRepository.Find(e => e.DepartmentId == department.DepartmentId);
            department.EmployeesCount = employeesCount.Count();
        }

        [HttpPost("CreateDepartment")]
        public async Task<ActionResult> CreateDepartment(CreateDepartmentRequest createRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Model is not valid" });
                }

                var existingDepartment = await _departmentRepository.GetByCondition(d => d.DepartmentName == createRequest.DepartmentName);
                if (existingDepartment != null)
                {
                    return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Department name already exists" });
                }

                var department = _mapper.Map<Departments>(createRequest);
                department.CreatedAt = DateTime.UtcNow;

                department.CreatedBy = User.FindFirstValue("UId");

                await _departmentRepository.Create(department);

                return StatusCode(StatusCodes.Status201Created, new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateDepartment/{id:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> UpdateDepartment(long id, CreateDepartmentRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Model is not valid" });
            }

            try
            {
                var department = await _departmentRepository.Get(id);
                if (department == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                department.DepartmentName = updateRequest.DepartmentName;
                department.ModifiedAt = DateTime.UtcNow;

                var userId = User.FindFirstValue("UId");
                department.ModifiedBy = userId;

                await _departmentRepository.Update(department);

                return Ok(new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAllDepartments")]
        public async Task<ActionResult<BaseResponse<IEnumerable<Departments>>>> GetDepartments()
        {
            try
            {
                var departments = await _departmentRepository.GetAll();

                foreach (var department in departments)
                {
                    await PopulateDepartmentDetails(department);
                }

                return Ok(new BaseResponse<IEnumerable<Departments>> { Success = true, Data = departments });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<IEnumerable<Departments>> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetDepartment/{id:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> GetDepartment(long id)
        {
            try
            {
                var department = await _departmentRepository.Get(id);
                if (department == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                await PopulateDepartmentDetails(department);

                return Ok(new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }

        [HttpPatch("SetDepartmentStatus/{id:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> SetDepartmentStatus(long id, [FromBody] int status)
        {
            if (status != 0 && status != 1)
            {
                return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Invalid status. Use 0 for deactivate or 1 for activate." });
            }

            try
            {
                var department = await _departmentRepository.Get(id);
                if (department == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                department.Active = status;
                department.ModifiedAt = DateTime.UtcNow;

                var userId = User.FindFirstValue("UId");
                department.ModifiedBy = userId;

                await _departmentRepository.Update(department);

                return Ok(new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("DeleteDepartment/{id:long}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteDepartment(long id)
        {
            try
            {
                var department = await _departmentRepository.Get(id);
                if (department == null)
                {
                    return NotFound(new BaseResponse<bool> { Success = false, Message = "Department not found" });
                }

                await _departmentRepository.Delete(department);

                return Ok(new BaseResponse<bool> { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<bool> { Success = false, Message = ex.Message });
            }
        }

        [HttpPatch("AssignDepartmentHead/{departmentId:long}/{managerId:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> AssignDepartmentHead(long departmentId, long managerId)
        {
            try
            {
                var department = await _departmentRepository.Get(departmentId);
                if (department == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                var manager = await _managerRepository.Get(managerId);
                if (manager == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Manager not found" });
                }

                var existingDepartment = await _departmentRepository.GetByCondition(d => d.ManagerId == managerId);
                if (existingDepartment != null)
                {
                    return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Manager is already assigned to another department" });
                }

                department.ManagerId = managerId;
                department.DepartmentHead = manager.FirstName;
                department.ModifiedAt = DateTime.UtcNow;

                var userId = User.FindFirstValue("UId");
                department.ModifiedBy = userId;

                await _departmentRepository.Update(department);

                manager.DepartmentId = departmentId;
                await _managerRepository.Update(manager);

                return Ok(new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }
    }
}
