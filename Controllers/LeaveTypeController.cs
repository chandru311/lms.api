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
    public class LeaveTypeController : ControllerBase
    {
        private readonly IGenericRepository<LeaveType> _leaveTypeRepository;
        private readonly IMapper _mapper;
        public LeaveTypeController(IGenericRepository<LeaveType> leaveTypeRepository, IMapper mapper)
        {
            _leaveTypeRepository = leaveTypeRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLeaveTypes()
        {
            BaseResponse<LeaveType> response = new();
            try
            {
                var allLeaveTypes = await _leaveTypeRepository.GetAll();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("/{Id:long}")]
        public async Task<IActionResult> GetLeaveTypeByID([FromRoute] long Id)
        {
            BaseResponse<LeaveType> response = new();
            try
            {
                var leaveType = await _leaveTypeRepository.Get(Id);
                if (leaveType == null)
                {
                    response.Message = "Leave Type not found";
                    return Ok(response);
                }
                response.Success = true;
                response.Data = leaveType;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveType reqModel)
        {
            BaseResponse<LeaveType> response = new();
            try
            {
                if (ModelState.IsValid)
                {
                    var loggedInUserId = "1";
                    var leaveExists = await _leaveTypeRepository.GetByCondition(x => x.Name == reqModel.Name);
                    if (leaveExists != null)
                    {
                        response.Message = "Leave Type Already Exists";
                        return Ok(response);
                    }

                    var leaveTypeMap = _mapper.Map<LeaveType>(reqModel);
                    leaveTypeMap.CreatedBy = loggedInUserId;
                    leaveTypeMap.CreatedAt = DateTime.Now;

                    await _leaveTypeRepository.Create(leaveTypeMap);

                    response.Success = true;
                    response.Message = "Created";
                }
                else
                {
                    response.Message = "Model is not valid";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("{Id:long}")]
        public async Task<IActionResult> UpdateLeaveType([FromRoute] long Id, [FromBody] CreateLeaveType reqModel)
        {
            BaseResponse<LeaveType> response = new();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.Message = "Model is not valid";
                    return Ok(response);
                }

                var loggedInUserId = "1";
                var leaveType = await _leaveTypeRepository.Get(Id);
                if (leaveType == null)
                {
                    response.Message = "Unable to get Leave Type";
                }
                else
                {
                    _mapper.Map(reqModel, leaveType);
                    leaveType.ModifiedAt = DateTime.Now;
                    leaveType.ModifiedBy = loggedInUserId;

                    await _leaveTypeRepository.Update(leaveType);

                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("Active_Deactive/{Id:long}")]
        public async Task<IActionResult> ChangeLeaveStatus(long Id, [FromQuery] bool isActive)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var leaveType = await _leaveTypeRepository.Get(Id);

                if (leaveType == null)
                {
                    resp.Message = "Leave Type Not Found";
                }
                else
                {
                    leaveType.Active = isActive ? 1 : 0;
                    await _leaveTypeRepository.Update(leaveType);
                    resp.Success = true;
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.Message;
            }

            return Ok(resp);
        }

        [HttpDelete("{Id:long}")]
        public async Task<IActionResult> DeleteLeaveType(long Id)
        {
            BaseResponse<Employees> resp = new();
            try
            {
                var leaveType = await _leaveTypeRepository.Get(Id);

                if (leaveType == null)
                {
                    resp.Message = "Leave Type Not Found";
                    return Ok(resp);
                }

                await _leaveTypeRepository.Delete(leaveType);
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
