﻿using Mapster;

using MapsterMapper;

using MediatR;

using MovieService.Domain.Exceptions;
using MovieService.Domain.Interfaces.Repositories.UnitOfWork;
using MovieService.Domain.Models;

namespace MovieService.Application.Handlers.Commands.Seats.UpdateSeatType;

public class UpdateSeatTypeCommandHandler(
	IUnitOfWork unitOfWork,
	IMapper mapper) : IRequestHandler<UpdateSeatTypeCommand, SeatTypeModel>
{
	private readonly IUnitOfWork _unitOfWork = unitOfWork;
	private readonly IMapper _mapper = mapper;

	public async Task<SeatTypeModel> Handle(UpdateSeatTypeCommand request, CancellationToken cancellationToken)
	{
		var existSeatType = await _unitOfWork.SeatsRepository.GetTypeAsync(request.Id, cancellationToken)
				?? throw new NotFoundException($"Seat type with id '{request.Id}' doesn't exists");

		request.Adapt(existSeatType);

		_unitOfWork.SeatsRepository.Update(existSeatType, cancellationToken);

		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return _mapper.Map<SeatTypeModel>(existSeatType);
	}
}