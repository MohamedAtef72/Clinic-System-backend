using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetRatingByAppointmentId(int appointmentId)
        {
            try
            {
                var rate = await _ratingService.GetRateByAppointmentIdAsync(appointmentId);

                if (rate == null)
                    return NotFound(new { Message = "No rating found for this appointment." });

                return Ok(new
                {
                    Message = "Rating retrieved successfully.",
                    Data = rate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving the rating.",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetRatingsByDoctorId(Guid doctorId)
        {
            try
            {
                var (averageRate, totalRatings) = await _ratingService.GetAverageRateByDoctorIdAsync(doctorId);

                if (totalRatings == 0)
                    return NotFound(new { Message = "No ratings found for this doctor." });

                return Ok(new
                {
                    Message = "Ratings retrieved successfully.",
                    Data = new
                    {
                        DoctorId = doctorId,
                        AverageRate = averageRate,
                        TotalRatings = totalRatings
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving doctor ratings.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateRating([FromBody] RatingCreateDTO ratingCreateModel)
        {
            try
            {
                if (ratingCreateModel == null)
                    return BadRequest(new { Message = "Rating data is required." });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid rating data.", Errors = ModelState });

                var result = await _ratingService.AddRateAsync(ratingCreateModel);

                if (!result)
                    return BadRequest(new { Message = "Failed to create rating. Check if appointment already has a rating." });

                return Ok(new { Message = "Rating created successfully." });
            }
            catch (ArgumentException ex)
            {
                // Custom validation issues (e.g., invalid appointment ID)
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // If the doctor/patient/appointment was not found
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while creating the rating.",
                    Error = ex.Message
                });
            }
        }

        // ------------------ Update Rating ------------------
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] RatingUpdateDTO ratingUpdateModel)
        {
            try
            {
                if (ratingUpdateModel == null)
                    return BadRequest(new { Message = "Rating data is required." });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid rating data.", Errors = ModelState });

                var result = await _ratingService.UpdateRateAsync(id, ratingUpdateModel);

                if (!result)
                    return NotFound(new { Message = "Rating not found or could not be updated." });

                return Ok(new { Message = "Rating updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while updating the rating.",
                    Error = ex.Message
                });
            }
        }
    }
}
