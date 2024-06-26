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
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LeaveController(IGenericRepository<Leave> leaveRepository, ApplicationDbContext context, IMapper mapper)
        {
            _leaveRepository = leaveRepository;
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("ApplyLeave")]
        [Authorize]
        public async Task<IActionResult> ApplyLeave([FromBody] ApplyLeaveRequest request)
        {
            BaseResponse<Leave> response = new BaseResponse<Leave>();

            try
            {
                if (ModelState.IsValid)
                {
                    var loggedInUserId = User.FindFirstValue("UId");
                    if (loggedInUserId == null)
                    {
                        response.Success = false;
                        response.Message = "Invalid User";
                        return Ok(response);
                    }

                    var leave = new Leave
                    {
                        EmployeeId = request.EmployeeId,
                        LeaveType = request.LeaveType,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Reason = request.Reason,
                        Status = request.Status,
                        CreatedAt = DateTime.Now,
                        CreatedBy = loggedInUserId,
                    };

                    await _leaveRepository.Create(leave);

                    response.Success = true;
                    response.Data = leave;
                    return Ok(response);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Model is not Valid";
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet("GetAllLeaves")]
        [Authorize]
        public async Task<IActionResult> GetAllLeaves()
        {
            BaseResponse<IEnumerable<Leave>> response = new BaseResponse<IEnumerable<Leave>>();

            try
            {
                var leaves = await _leaveRepository.Find(l => l.Status != LeaveStatus.Rejected);
                response.Success = true;
                response.Data = leaves;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet("GetLeavesByEmployeeId/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetLeavesByEmployeeId(long employeeId)
        {
            BaseResponse<IEnumerable<Leave>> response = new BaseResponse<IEnumerable<Leave>>();

            try
            {
                var leaves = await _leaveRepository.Find(l => l.EmployeeId == employeeId && l.Status != LeaveStatus.Rejected);
                response.Success = true;
                response.Data = leaves;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Ok(response);
            }
        }

        [HttpPut("UpdateLeave/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateLeave(int id, [FromBody] ApplyLeaveRequest request)
        {
            BaseResponse<Leave> response = new BaseResponse<Leave>();

            try
            {
                if (ModelState.IsValid)
                {
                    var leave = await _leaveRepository.GetID(id);
                    if (leave == null)
                    {
                        response.Success = false;
                        response.Message = "Leave request not found";
                        return Ok(response);
                    }

                    leave.LeaveType = request.LeaveType;
                    leave.FromDate = request.FromDate;
                    leave.ToDate = request.ToDate;
                    leave.Reason = request.Reason;
                    leave.Status = request.Status;

                    await _leaveRepository.Update(leave);

                    response.Success = true;
                    response.Data = leave;
                    return Ok(response);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Model is not Valid";
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Ok(response);
            }
        }

        [HttpDelete("DeleteLeave/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            BaseResponse<bool> response = new BaseResponse<bool>();

            try
            {
                var leave = await _leaveRepository.GetID(id);
                if (leave == null)
                {
                    response.Success = false;
                    response.Message = "Leave request not found";
                    return Ok(response);
                }

                await _leaveRepository.Delete(leave);

                response.Success = true;
                response.Data = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Ok(response);
            }
        }
    }
}
