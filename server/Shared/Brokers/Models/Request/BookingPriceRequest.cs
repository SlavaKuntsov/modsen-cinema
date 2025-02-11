﻿using BookingService.Domain.Models;

namespace Brokers.Models.Request;

public class BookingPriceRequest
{
	public Guid SessionId { get; private set; }
	public IList<SeatModel> Seats { get; private set; } = [];

	public BookingPriceRequest(
		Guid sessionId,
		IList<SeatModel> seats)
	{
		SessionId = sessionId;
		Seats = seats;
	}
}