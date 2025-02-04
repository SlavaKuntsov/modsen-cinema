﻿using BookingService.Application.Handlers.Commands.Bookings.CancelBooking;
using BookingService.Application.Handlers.Commands.Bookings.CreateBooking;
using BookingService.Application.Handlers.Commands.Bookings.PayBooking;
using BookingService.Application.Handlers.Commands.Seats.UpdateSeats;
using BookingService.Application.Handlers.Query.Bookings.GetAllBookings;
using BookingService.Application.Handlers.Query.Bookings.GetBookingsByUserId;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers.Http;

[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<BookingController> _logger;

	public BookingController(IMediator mediator, ILogger<BookingController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	[HttpGet("/Bookings")]
	public async Task<IActionResult> Get()
	{
		_logger.LogInformation("Fetch all bookings.");

		var bookings = await _mediator.Send(new GetAllBookingsQuery());

		_logger.LogInformation("Successfully fetched {Count} bookings.", bookings.Count);

		return Ok(bookings);
	}

	[HttpGet("/Bookings/{id:Guid}")]
	public async Task<IActionResult> Get([FromRoute] Guid id)
	{
		var bookings = await _mediator.Send(new GetUserBookingsByIdQuery(id));

		return Ok(bookings);
	}

	[HttpPost("/Bookings")]
	public async Task<IActionResult> Create([FromBody] CreateBookingCommand request)
	{
		_logger.LogInformation($"Starting to create bookings {request.UserId} - {request.SessionId}.");

		var bookingId = await _mediator.Send(request);

		_logger.LogInformation($"Processed create bookings {request.UserId} - {request.SessionId}.");

		return Ok(bookingId);
	}

	[HttpPatch("/Bookings/Pay/{bookingId:Guid}/User/{userId:Guid}")]
	public async Task<IActionResult> Pay([FromRoute] Guid bookingId, [FromRoute] Guid userId)
	{
		var booking = await _mediator.Send(new PayBookingCommand(bookingId, userId));

		return Ok(booking);
	}

	[HttpPatch("/Bookings/Cancel/{bookingId:Guid}")]
	public async Task<IActionResult> Cancel([FromRoute] Guid bookingId)
	{
		var booking = await _mediator.Send(new CancelBookingCommand(bookingId));

		await _mediator.Send(new UpdateSeatsCommand(
			booking.SessionId,
			booking.Seats,
			false));

		return Ok(booking);
	}
}