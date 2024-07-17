using AutoMapper;
using lms.api.Data;
using lms.api.Models;
using lms.api.Models.RequestModels;
using lms.api.Models.ResponseModels;
using lms.api.Repository;
using lms.api.Types;
using Microsoft.AspNetCore.Mvc;

namespace lms.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly IGenericRepository<PublicHolidays> _publicHolidayRepository;
        private readonly IGenericRepository<Leaves> _leaveRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LeaveController(
            IGenericRepository<PublicHolidays> publicHolidayRepository,
            IGenericRepository<Leaves> leaveRepository,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _leaveRepository = leaveRepository;
            _publicHolidayRepository = publicHolidayRepository;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("GetAllPublicHolidays")]
        public async Task<IActionResult> GetAllHolidays()
        {
            BaseResponse<IEnumerable<PublicHolidays>> response = new();
            try
            {
                var holidays = await _publicHolidayRepository.GetAll();
                response.Success = true;
                response.Data = holidays;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return Ok(response);
        }

        [HttpGet("GetAllLeaves")]
        public async Task<IActionResult> GetAllLeaves()
        {
            try
            {
                var leaves = await _leaveRepository.GetAll();
                return Ok(new BaseResponse<IEnumerable<Leaves>> { Success = true, Data = leaves });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<Leaves>> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetLeavesByAiId/{AiId:long}")]
        public async Task<IActionResult> GetLeavesByEmployeeId(long AiId)
        {
            try
            {
                var leaves = await _leaveRepository.Find(l => l.AiId == AiId);
                if (leaves == null)
                {
                    return NotFound(new BaseResponse<IEnumerable<Leaves>> { Success = false, Message = "No leave requests found for the given AiID" });
                }
                return Ok(new BaseResponse<IEnumerable<Leaves>> { Success = true, Data = leaves });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<Leaves>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("ApplyLeave")]
        public async Task<IActionResult> ApplyLeave([FromBody] CreateLeaveRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loggedInUserId = "1";
                    if (loggedInUserId == null)
                    {
                        return Unauthorized(new BaseResponse<Leaves> { Success = false, Message = "Invalid User" });
                    }

                    var leave = new Leaves
                    {
                        AiId = request.AiId,
                        LeaveType = request.LeaveType,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Reason = request.Reason,
                        Status = request.Status,
                        CreatedAt = DateTime.Now,
                        CreatedBy = request.AiId.ToString(),
                    };

                    await _leaveRepository.Create(leave);

                    return Ok(new BaseResponse<Leaves> { Success = true, Data = leave });
                }
                else
                {
                    return BadRequest(new BaseResponse<Leaves> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Leaves> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateLeave/{LeaveId:long}")]
        public async Task<IActionResult> UpdateLeave(long LeaveId, [FromBody] CreateLeaveRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var leave = await _leaveRepository.Get(LeaveId);
                    if (leave == null)
                    {
                        return NotFound(new BaseResponse<Leaves> { Success = false, Message = "Leave request not found" });
                    }

                    if (leave.Status != LeaveStatus.Pending)
                    {
                        return BadRequest(new BaseResponse<Leaves> { Success = false, Message = "Only pending leave requests can be updated" });
                    }

                    leave.LeaveType = request.LeaveType;
                    leave.FromDate = request.FromDate;
                    leave.ToDate = request.ToDate;
                    leave.Reason = request.Reason;
                    leave.ModifiedAt = DateTime.UtcNow;
                    leave.ModifiedBy = request.AiId.ToString();

                    await _leaveRepository.Update(leave);

                    return Ok(new BaseResponse<Leaves> { Success = true, Data = leave });
                }
                else
                {
                    return BadRequest(new BaseResponse<Leaves> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Leaves> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("Approve_Reject/{LeaveId:long}")]
        public async Task<IActionResult> ApplyOrRejectLeave(long LeaveId, [FromBody] UpdateLeaveStatusRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var leave = await _leaveRepository.Get(LeaveId);
                    if (leave == null)
                    {
                        return NotFound(new BaseResponse<Leaves> { Success = false, Message = "Leave request not found" });
                    }

                    if (leave.Status == LeaveStatus.Rejected)
                    {
                        return BadRequest(new BaseResponse<Leaves> { Success = false, Message = "Cannot update an rejected leave request" });
                    }

                    if (request.Status != (int)LeaveStatus.Approved && request.Status != (int)LeaveStatus.Rejected)
                    {
                        return BadRequest(new BaseResponse<Leaves> { Success = false, Message = "Invalid status. Only approved or rejected statuses are allowed" });
                    }

                    leave.Status = (LeaveStatus)request.Status;
                    await _leaveRepository.Update(leave);

                    return Ok(new BaseResponse<Leaves> { Success = true, Data = leave });
                }
                else
                {
                    return BadRequest(new BaseResponse<Leaves> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Leaves> { Success = false, Message = ex.Message });
            }
        }

        /*  [HttpDelete("DeleteLeave/{LeaveId:long}")]
          public async Task<IActionResult> DeleteLeave(long LeaveId)
          {
              try
              {
                  var leave = await _leaveRepository.Get(LeaveId);
                  if (leave == null)
                  {
                      return NotFound(new BaseResponse<bool> { Success = false, Message = "Leave request not found" });
                  }

                  if (leave.Status != LeaveStatus.Pending)
                  {
                      return BadRequest(new BaseResponse<bool> { Success = false, Message = "Only pending leave requests can be deleted" });
                  }

                  await _leaveRepository.Delete(leave);

                  return NoContent();
              }
              catch (Exception ex)
              {
                  return StatusCode(500, new BaseResponse<bool> { Success = false, Message = ex.Message });
              }
          }*/
    }
}
