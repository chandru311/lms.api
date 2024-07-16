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
    public class AddressController : ControllerBase
    {
        private readonly IGenericRepository<Address> _addressRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public AddressController(
            IGenericRepository<Address> addressRepository,
            IMapper mapper,
            ApplicationDbContext context)
        {
            _addressRepository = addressRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("GetAllAddresses")]
        public async Task<IActionResult> GetAllAddresses()
        {
            try
            {
                var addresses = await _addressRepository.GetAll();
                return Ok(new BaseResponse<IEnumerable<Address>> { Success = true, Data = addresses });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<Address>> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAddressById/{id:long}")]
        public async Task<IActionResult> GetAddressById(long id)
        {
            try
            {
                var address = await _addressRepository.Get(id);
                if (address == null)
                {
                    return NotFound(new BaseResponse<Address> { Success = false, Message = "Address not found" });
                }
                return Ok(new BaseResponse<Address> { Success = true, Data = address });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Address> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAddressesByEmployeeId/{aiId:long}")]
        public async Task<IActionResult> GetAddressesByEmployeeId(long aiId)
        {
            try
            {
                var addresses = await _addressRepository.Find(a => a.AiId == aiId);
                if (addresses == null || !addresses.Any())
                {
                    return NotFound(new BaseResponse<IEnumerable<Address>> { Success = false, Message = "No addresses found for the given employee ID" });
                }
                return Ok(new BaseResponse<IEnumerable<Address>> { Success = true, Data = addresses });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<IEnumerable<Address>> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("CreateAddress")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loggedInUserId = User.FindFirstValue("AiId");
                    if (loggedInUserId == null)
                    {
                        return Unauthorized(new BaseResponse<Address> { Success = false, Message = "Invalid User" });
                    }

                    var address = _mapper.Map<Address>(request);
                    address.CreatedAt = DateTime.Now;
                    address.CreatedBy = loggedInUserId;

                    await _addressRepository.Create(address);

                    return Ok(new BaseResponse<Address> { Success = true, Data = address });
                }
                else
                {
                    return BadRequest(new BaseResponse<Address> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Address> { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateAddress/{id:long}")]
        public async Task<IActionResult> UpdateAddress(long id, [FromBody] CreateAddressRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var address = await _addressRepository.Get(id);
                    if (address == null)
                    {
                        return NotFound(new BaseResponse<Address> { Success = false, Message = "Address not found" });
                    }

                    _mapper.Map(request, address);
                    var loggedInUserId = User.FindFirstValue("AiId");
                    address.ModifiedAt = DateTime.Now;
                    address.ModifiedBy = loggedInUserId;

                    await _addressRepository.Update(address);

                    return Ok(new BaseResponse<Address> { Success = true, Data = address });
                }
                else
                {
                    return BadRequest(new BaseResponse<Address> { Success = false, Message = "Model is not valid" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<Address> { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("DeleteAddress/{id:long}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            try
            {
                var address = await _addressRepository.Get(id);
                if (address == null)
                {
                    return NotFound(new BaseResponse<bool> { Success = false, Message = "Address not found" });
                }

                await _addressRepository.Delete(address);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResponse<bool> { Success = false, Message = ex.Message });
            }
        }
    }
}
