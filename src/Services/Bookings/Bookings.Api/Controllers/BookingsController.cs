using Bookings.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bookings.Api.Controllers;

[ApiController]
[Route("bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookings;

    public BookingsController(IBookingService bookings)
    {
        _bookings = bookings;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var booking = await _bookings.CreateBookingAsync(
            request.EventId, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = booking.Id },
            booking);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var booking = await _bookings.GetBookingByIdAsync(id);
        return Ok(booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        await _bookings.CancelBookingAsync(
            id,
            userId,
            User.IsInRole("Admin"));

        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(claim, out userId);
    }
}

public record CreateBookingRequest(Guid EventId);