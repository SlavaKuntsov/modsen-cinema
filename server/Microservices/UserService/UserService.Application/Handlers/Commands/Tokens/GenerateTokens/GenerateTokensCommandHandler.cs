﻿using MapsterMapper;

using MediatR;

using UserService.Application.DTOs;
using UserService.Application.Interfaces.Auth;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces.Repositories;
using UserService.Domain.Models;

namespace UserService.Application.Handlers.Commands.Tokens.GenerateAndUpdateTokens;

public class GenerateTokensCommandHandler(ITokensRepository tokensRepository, IJwt jwt, IMapper mapper) : IRequestHandler<GenerateTokensCommand, AuthDto>
{
	private readonly ITokensRepository _tokensRepository = tokensRepository;
	private readonly IJwt _jwt = jwt;
	private readonly IMapper _mapper = mapper;

	public async Task<AuthDto> Handle(GenerateTokensCommand request, CancellationToken cancellationToken)
	{
		var accessToken = _jwt.GenerateAccessToken(request.Id, request.Role);
		var newRefreshToken = _jwt.GenerateRefreshToken();

		var refreshTokenModel = new RefreshTokenModel(
				request.Id,
				request.Role,
				newRefreshToken,
				_jwt.GetRefreshTokenExpirationDays());

		await _tokensRepository.SetRefreshTokenAsync(
			request.Id,
			request.Role,
			_mapper.Map<RefreshTokenEntity>(refreshTokenModel),
			cancellationToken);

		return new AuthDto(accessToken, newRefreshToken);
	}
}