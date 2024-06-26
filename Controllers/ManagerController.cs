using AutoMapper;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IGenericRepository<Managers> _managerRepository;
        private readonly IMapper _mapper;
        public ManagerController(IGenericRepository<Managers> managerRepository, IMapper mapper)
        {
            _managerRepository = managerRepository;
            _mapper = mapper;
        }

        [HttpGet("{ManagerId:long}")]
        [Authorize]
        public async Task<IActionResult> GetManagerById([FromRoute]long ManagerId)
        {
            BaseResponse<Managers> response = new();
            try
            {
                var manager = await _managerRepository.Get(ManagerId);
                if (manager == null)
                {
                    response.Message = "Manager Not Found";
                }
                return Ok(manager);
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
            BaseResponse<Managers> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var Manager = _managerRepository.IsRecordExists(x => x.EmployeeId == reqModel.EmployeeId);

                    if (!Manager)
                    {
                        response.Message = "EmployeeId Already Exits";
                    }
                }
                else
                {
                    response.Message = "Model is not valid";
                }
                return Ok(response);
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
        }
    }
}
