using EventApi.Application.DTOs;
using EventApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using WebApiProject.Responses;


namespace WebApiProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            var result = new ApiResult<BookingInfo>
            {
                Data = booking,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Получение информации о брони"
            };

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userIdClaim = User.FindFirstValue(
                JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            await _bookingService.CancelBookingAsync(
                id,
                userId,
                isAdmin);

            return NoContent();
        }
    }
}
