﻿using MapsterMapper;

using MediatR;

using MovieService.Domain.Entities;
using MovieService.Domain.Exceptions;
using MovieService.Domain.Interfaces.Repositories;
using MovieService.Domain.Models;

namespace MovieService.Application.Handlers.Commands.Halls.CreateHall;

public class CreateHallCommandHandler(
	IHallsRepository hallsRepository,
	IMapper mapper) : IRequestHandler<CreateHallCommand, Guid>
{
	private readonly IHallsRepository _hallsRepository = hallsRepository;
	private readonly IMapper _mapper = mapper;

	public async Task<Guid> Handle(CreateHallCommand request, CancellationToken cancellationToken)
	{
		var seatCount = request.Seats.Sum(row =>
			row.Count(seat => seat == 1 || seat == 0));

		if (seatCount != request.TotalSeats)
			throw new InvalidOperationException(
				$"The number of seats ({seatCount}) does not match the specified total seats ({request.TotalSeats}).");

		var rowLength = request.Seats.FirstOrDefault()?.Length ?? 0;

		if (request.Seats.Any(row => row.Length != rowLength))
			throw new InvalidOperationException("All rows in seats must have the same length.");

		var existHall = await _hallsRepository.GetAsync(request.Name, cancellationToken);

		if (existHall is not null)
			throw new AlreadyExistsException($"Hall with name '{request.Name}' already exist.");

		var hall = new HallModel(
			Guid.NewGuid(),
			request.Name,
			request.TotalSeats,
			request.Seats);

		await _hallsRepository.CreateAsync(_mapper.Map<HallEntity>(hall), cancellationToken);

		return hall.Id;
	}
}