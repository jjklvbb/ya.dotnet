using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApiProject.DTOs;
using WebApiProject.Interfaces;
using WebApiProject.Responses;

namespace WebApiProject.Controllers
{
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
    }
}
