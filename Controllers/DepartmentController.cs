using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Managers> _managersRepository;
        private readonly IMapper _mapper;
        private string _loggedInUserId;
        public DepartmentController(IGenericRepository<Departments> departmentRepository, IMapper mapper,
            IGenericRepository<Managers> managersRepository)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
            _managersRepository = managersRepository;
        }

        private void GetLoggedInUserId()
        {
            _loggedInUserId = User.FindFirstValue("UId");
        }

        [HttpGet("{DepartmentId:long}")]
        [Authorize]
        public async Task<IActionResult> GetDepartment([FromRoute] long DepartmentId)
        {
            BaseResponse<Departments> response = new();
            try
            {
                var dept = await _departmentRepository.Get(DepartmentId);
                if (dept == null)
                {
                    response.Message = "No Department Found";
                    response.Success = false;
                    return Ok(response);
                }
                return Ok(dept);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllDepts()
        {
            BaseResponse<Departments> response = new();
            try
            {
                var depts = await _departmentRepository.GetAll();
                return Ok(depts);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateDept([FromBody] CreateDepartmentRequest reqModel)
        {
            GetLoggedInUserId();
            BaseResponse<Departments> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var isDeptExist = _departmentRepository.IsRecordExists(x => x.DepartmentName == reqModel.DepartmentName);
                    var isManagerexists = _managersRepository.IsRecordExists(x => x.ManagerId == reqModel.ManagerId);
                    if (!isDeptExist)
                    {
                        response.Success = false;
                        response.Message = "Department Already Exists";
                        return Ok(response);
                    }

                    if (!isManagerexists)
                    {
                        response.Success = false;
                        response.Message = "Manager doesn't exist";
                        return Ok(response);
                    }

                    var dept = _mapper.Map<Departments>(reqModel);
                    dept.Active = 1;
                    dept.CreatedBy = _loggedInUserId;
                    dept.CreatedAt = DateTime.Now;

                    await _departmentRepository.Create(dept);

                    response.Success = true;
                    return Ok(response);
                }
                else
                {
                    response.Message = "Model is not Valid";
                    response.Success = false;
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("{DepartmentId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateDept([FromRoute]long DepartmentId, [FromBody]CreateDepartmentRequest reqModel)
        {
            GetLoggedInUserId();
            BaseResponse<Departments> response = new();
            try
            {
                if(ModelState.IsValid)
                {
                    var dept = await _departmentRepository.Get(DepartmentId);
                    if(dept == null)
                    {
                        response.Message = "No Department Found";
                    }

                    var deptMap = _mapper.Map<Departments>(reqModel);
                    dept.ModifiedBy = _loggedInUserId;
                    dept.ModifiedAt = DateTime.Now;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Model is not Valid";
                }
            }
            catch (Exception ex)
            {
                response.Message= ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("ActivateDept")]
        [Authorize]
        public async Task<IActionResult> ActivateDept([FromRoute] long DepartmentId)
        {
            BaseResponse<Departments> response = new();
            try
            {
                var dept = await _departmentRepository.Get(DepartmentId);
                if (dept == null)
                {
                    response.Success = false;
                    response.Message = "No Department Found";
                }

                dept.Active = 1;
                await _departmentRepository.Update(dept);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpDelete("DeactivateDept")]
        [Authorize]
        public async Task<IActionResult> DeactivateDept([FromRoute]long DepartmentId)
        {
            BaseResponse<Departments> response = new();
            try
            {
                var dept = await _departmentRepository.Get(DepartmentId);
                if (dept == null)
                {
                    response.Success = false;
                    response.Message = "No Department Found";
                }

                dept.Active = 0;
                await _departmentRepository.Update(dept);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
