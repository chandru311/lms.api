using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Managers> _managerRepository;
        private readonly IGenericRepository<Employees> _employeeRepository;
        private readonly IGenericRepository<Usermaster> _userRepository;
        private readonly IMapper _mapper;

        public DepartmentsController(
             IGenericRepository<Usermaster> userRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Managers> managerRepository,
            IGenericRepository<Employees> employeeRepository,
            IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _managerRepository = managerRepository;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }
        private async Task PopulateDepartmentDetails(Departments department)
        {
            if (department.DepartmentHeadId != 0)
            {
                var manager = await _managerRepository.GetByCondition(m => m.AiId == department.DepartmentHeadId);
                if (manager != null)
                {
                    department.DepartmentHead = manager.FirstName;
                }
            }

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
                department.CreatedBy = "Admin";
                department.CreatedAt = DateTime.UtcNow;

                await _departmentRepository.Create(department);

                return StatusCode(StatusCodes.Status201Created, new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateDepartment/{DepartmentId:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> UpdateDepartment(long DepartmentId, CreateDepartmentRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Model is not valid" });
            }

            try
            {
                var checkDepartment = await _departmentRepository.Get(DepartmentId);
                if (checkDepartment == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                var department = _mapper.Map<Departments>(updateRequest);
                department.ModifiedBy = "Admin";
                department.ModifiedAt = DateTime.UtcNow;

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

        [HttpGet("GetDepartment/{DepartmentId:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> GetDepartment(long DepartmentId)
        {
            try
            {
                var department = await _departmentRepository.Get(DepartmentId);
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

        [HttpPatch("Active_Deactive/{DepartmentId:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> ChangeDepartmentStatus(long DepartmentId, [FromBody] int status)
        {
            if (status != 0 && status != 1)
            {
                return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Invalid status. Use 0 for deactivate or 1 for activate." });
            }

            try
            {
                var department = await _departmentRepository.Get(DepartmentId);
                if (department == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                department.Active = status;
                department.ModifiedBy = "Admin";
                department.ModifiedAt = DateTime.UtcNow;

                await _departmentRepository.Update(department);

                return Ok(new BaseResponse<Departments> { Success = true, Data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse<Departments> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("DeleteDepartment/{DepartmentId:long}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteDepartment(long DepartmentId)
        {
            try
            {
                var department = await _departmentRepository.Get(DepartmentId);
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

        [HttpPatch("AssignDepartmentHead/{DepartmentId:long}/{AiId:long}")]
        public async Task<ActionResult<BaseResponse<Departments>>> AssignDepartmentHead(long DepartmentId, long AiId)
        {
            try
            {
                var department = await _departmentRepository.Get(DepartmentId);
                if (department == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Department not found" });
                }

                var manager = await _managerRepository.GetByCondition(x => x.AiId == AiId);
                if (manager == null)
                {
                    return NotFound(new BaseResponse<Departments> { Success = false, Message = "Manager not found" });
                }

                var managerUser = await _userRepository.GetByCondition(u => u.AiId == manager.AiId && u.Active == 1 && u.UserType == 2);
                if (managerUser == null)
                {
                    return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Invalid user. Only active managers  can be department head." });
                }

                var existingDepartment = await _departmentRepository.GetByCondition(d => d.DepartmentHeadId == manager.AiId && d.Active == 1);
                if (existingDepartment != null)
                {
                    return BadRequest(new BaseResponse<Departments> { Success = false, Message = "Manager is already assigned as department head in another active department" });
                }

                department.DepartmentHead = manager.FirstName;
                department.DepartmentHeadId = manager.AiId;
                department.ModifiedAt = DateTime.UtcNow;
                department.ModifiedBy = "Admin";

                await _departmentRepository.Update(department);

                manager.DepartmentId = DepartmentId;
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
