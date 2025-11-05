using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboardInfo()
        {
            try
            {
                var dashboardInfo = await _adminService.GetDashboardInfo();
                if (dashboardInfo == null)
                {
                    return NotFound("Data Not Found");
                }

                return Ok(new {Message = "Data Retreive Successfully" , Data = dashboardInfo});
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
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {

                var model = await _adminService.GetRecentActivityData();

                if (model == null)
                {
                    return NotFound("Data Not Found");
                }

                return Ok(new {Message = "Data Retrieve Successfully" , Data = model});

            }catch (Exception ex)
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
