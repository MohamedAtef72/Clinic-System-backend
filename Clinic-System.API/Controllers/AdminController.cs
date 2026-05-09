using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Runtime.CompilerServices;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ICacheService _cache;

        public AdminController(IAdminService adminService, ICacheService cache)
        {
            _adminService = adminService;
            _cache = cache;
        }

        [HttpGet("Dashboard")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetDashboardInfo()
        {
            try
            {
               var freshData = await _adminService.GetDashboardInfo();

               if (freshData == null)
                   return NotFound("Data Not Found");

               return Ok(new { Message = "Data Retreive Successfully", Data = freshData });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "An unexpected error occurred while get Data.",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("RecentData")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var freshData = await _adminService.GetRecentActivityData();

                if (freshData == null)
                    return NotFound("Data Not Found");

                return Ok(new { Message = "Data Retrieve Successfully", Data = freshData });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = "An unexpected error occurred while get Data.",
                    Error = ex.Message
                });
            }
        }
    }
}
