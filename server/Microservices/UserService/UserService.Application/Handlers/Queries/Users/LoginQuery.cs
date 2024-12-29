﻿using MediatR;

using UserService.Application.Interfaces.Auth;
using UserService.Domain;
using UserService.Domain.Exceptions;
using UserService.Domain.Interfaces.Repositories;

namespace UserService.Application.Handlers.Queries.Users;

public class LoginQuery(string email, string password) : IRequest<UserModel>
{
	public string Email { get; private set; } = email;
	public string Password { get; private set; } = password;

	public class LoginQUeryHandler(IUsersRepository usersRepository,
								//ILogger<LoginQuery> logger,
								IPasswordHash passwordHash) : IRequestHandler<LoginQuery, UserModel>
	{
		private readonly IPasswordHash _passwordHash = passwordHash;
		private readonly IUsersRepository _usersRepository = usersRepository;
		//private readonly ILogger<LoginQuery> _logger = logger;

		public async Task<UserModel> Handle(LoginQuery request, CancellationToken cancellationToken)
		{
			//_logger.LogInformation($"Start handling {request.GetType().Name} for user with Email {request.Email}");

			var existUser = await _usersRepository.GetAsync(request.Email, cancellationToken)
				?? throw new NotFoundException("User not found");

			var isCorrectPassword = _passwordHash.Verify(request.Password, existUser.Password);

			if (!isCorrectPassword)
				throw new UnauthorizedAccessException("Incorrect password");

			//_logger.LogInformation($"Successfully handled {request.GetType().Name} for user ID {existUser.Id}");

			return existUser;
		}
	}
}