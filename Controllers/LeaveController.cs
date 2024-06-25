using AutoMapper;
using lms.api.Data;
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
    public class LeaveController : ControllerBase
    {
        private readonly IGenericRepository<Leave> _leaveRepository;
        private readonly IGenericRepository<LeaveHistory> _leaveHistoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LeaveController(
            IGenericRepository<Leave> leaveRepository,
            IGenericRepository<LeaveHistory> leaveHistoryRepository,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _leaveRepository = leaveRepository;
            _leaveHistoryRepository = leaveHistoryRepository;
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
                        LeaveType = request.LeaveType,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Reason = request.Reason,
                        EmployeeId = request.EmployeeId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = loggedInUserId,
                        Status = request.Status
                    };

                    await _leaveRepository.Create(leave);

                    var leaveHistory = new LeaveHistory
                    {
                        EmployeeId = request.EmployeeId,
                        LeaveAppliedDate = DateTime.Now,
                        LeaveType = request.LeaveType,
                        Status = request.Status
                    };

                    await _leaveHistoryRepository.Create(leaveHistory);

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
    }
}
