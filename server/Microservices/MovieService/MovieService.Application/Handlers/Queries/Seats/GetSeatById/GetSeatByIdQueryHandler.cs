﻿using MapsterMapper;

using MediatR;

using MovieService.Domain.Interfaces.Repositories.UnitOfWork;
using MovieService.Domain.Models;

namespace MovieService.Application.Handlers.Queries.Seats.GetSeatById;

public class GetSeatByIdQueryHandler(
	IUnitOfWork unitOfWork,
	IMapper mapper) : IRequestHandler<GetSeatByIdQuery, SeatModel>
{
	private readonly IUnitOfWork _unitOfWork = unitOfWork;
	private readonly IMapper _mapper = mapper;

	public async Task<SeatModel> Handle(GetSeatByIdQuery request, CancellationToken cancellationToken)
	{
		var seats = await _unitOfWork.SeatsRepository.GetAsync(request.Id, cancellationToken);

		return _mapper.Map<SeatModel>(seats);
	}
}