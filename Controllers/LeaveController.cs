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
    public class LeaveController : ControllerBase
    {
        private readonly IGenericRepository<Leave> _leaveRepository;
        private readonly IGenericRepository<LeaveSum> _leaveSumRepository;
        private readonly IGenericRepository<PublicHolidays> _publicHolidayRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LeaveController(
            IGenericRepository<Leave> leaveRepository,
            IGenericRepository<LeaveSum> leaveSumRepository,
            IGenericRepository<PublicHolidays> publicHolidayRepository,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _leaveRepository = leaveRepository;
            _leaveSumRepository = leaveSumRepository;
            _publicHolidayRepository = publicHolidayRepository;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("GetAllPublicHolidays")]
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> GetAllLeaves()
        {
            try
            {
                var leaves = await _leaveRepository.GetAll();
                return Ok(new BaseResponse<IEnumerable<Leave>> { Success = true, Data = leaves });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<Leave>> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetLeavesByEmployeeId/{AiId:long}")]
        [Authorize]
        public async Task<IActionResult> GetLeavesByEmployeeId(long AiId)
        {
            try
            {
                var leaves = await _leaveRepository.Find(l => l.AiId == AiId);
                if (leaves == null || !leaves.Any())
                {
                    return NotFound(new BaseResponse<IEnumerable<Leave>> { Success = false, Message = "No leave requests found for the given employee ID" });
                }
                return Ok(new BaseResponse<IEnumerable<Leave>> { Success = true, Data = leaves });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<Leave>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("ApplyLeave")]
        [Authorize]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loggedInUserId = User.FindFirstValue("AiId");
                    if (loggedInUserId == null)
                    {
                        return Unauthorized(new BaseResponse<Leave> { Success = false, Message = "Invalid User" });
                    }

                    var leave = new Leave
                    {
                        AiId = request.AiId,
                        LeaveType = request.LeaveType,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Reason = request.Reason,
                        Status = request.Status,
                        CreatedAt = DateTime.Now,
                        CreatedBy = loggedInUserId,
                    };

                    await _leaveRepository.Create(leave);

                    return Ok(new BaseResponse<Leave> { Success = true, Data = leave });
                }
                else
                {
                    return BadRequest(new BaseResponse<Leave> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Leave> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateLeave/{LeaveId:long}")]
        [Authorize]
        public async Task<IActionResult> UpdateLeave(long LeaveId, [FromBody] ApplyLeaveRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var leave = await _leaveRepository.Get(LeaveId);
                    if (leave == null)
                    {
                        return NotFound(new BaseResponse<Leave> { Success = false, Message = "Leave request not found" });
                    }

                    if (leave.Status != LeaveStatus.Pending)
                    {
                        return BadRequest(new BaseResponse<Leave> { Success = false, Message = "Only pending leave requests can be updated" });
                    }

                    leave.LeaveType = request.LeaveType;
                    leave.FromDate = request.FromDate;
                    leave.ToDate = request.ToDate;
                    leave.Reason = request.Reason;
                    leave.Status = request.Status;

                    await _leaveRepository.Update(leave);

                    return Ok(new BaseResponse<Leave> { Success = true, Data = leave });
                }
                else
                {
                    return BadRequest(new BaseResponse<Leave> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Leave> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("Approve_RejectLeave/{LeaveId:long}")]
        [Authorize]
        public async Task<IActionResult> ApplyOrRejectLeave(long LeaveId, [FromBody] UpdateLeaveStatusRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var leave = await _leaveRepository.Get(LeaveId);
                    if (leave == null)
                    {
                        return NotFound(new BaseResponse<Leave> { Success = false, Message = "Leave request not found" });
                    }

                    if (leave.Status == LeaveStatus.Rejected)
                    {
                        return BadRequest(new BaseResponse<Leave> { Success = false, Message = "Cannot update an already rejected leave request" });
                    }

                    if (request.Status != (int)LeaveStatus.Approved && request.Status != (int)LeaveStatus.Rejected)
                    {
                        return BadRequest(new BaseResponse<Leave> { Success = false, Message = "Invalid status. Only approved or rejected statuses are allowed" });
                    }

                    if (request.Status == (int)LeaveStatus.Approved)
                    {
                        var leaveSum = await _leaveSumRepository.GetByCondition(x => x.AiId == leave.AiId);
                        if (leaveSum != null)
                        {
                            switch (leave.LeaveType)
                            {
                                case "SickLeave":
                                    leaveSum.SickLeave -= 1;
                                    leaveSum.LeavesTaken += 1;
                                    break;
                                case "CasualLeave":
                                    leaveSum.CasualLeave -= 1;
                                    leaveSum.LeavesTaken += 1;
                                    break;
                                case "PaidLeave":
                                    leaveSum.PaidLeave -= 1;
                                    leaveSum.LeavesTaken += 1;
                                    break;
                                case "UnpaidLeave":
                                case "Others":
                                    leaveSum.LeavesTaken += 1;
                                    break;
                            }
                            await _leaveSumRepository.Update(leaveSum);
                        }
                    }

                    leave.Status = (LeaveStatus)request.Status;
                    await _leaveRepository.Update(leave);

                    return Ok(new BaseResponse<Leave> { Success = true, Data = leave });
                }
                else
                {
                    return BadRequest(new BaseResponse<Leave> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Leave> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("DeleteLeave/{LeaveId:long}")]
        [Authorize]
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
        }
    }
}
