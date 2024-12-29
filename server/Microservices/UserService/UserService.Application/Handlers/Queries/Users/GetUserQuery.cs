﻿using MediatR;

using UserService.Domain;
using UserService.Domain.Interfaces.Repositories;

namespace UserService.Application.Handlers.Queries.Users;

public class GetUserQuery(Guid id) : IRequest<UserModel?>
{
	public Guid Id { get; init; } = id;

	public class GetUserQueryHandler(IUsersRepository usersRepository) : IRequestHandler<GetUserQuery, UserModel?>
	{
		private readonly IUsersRepository _usersRepository = usersRepository;

		public async Task<UserModel?> Handle(GetUserQuery request, CancellationToken cancellationToken)
		{
			return await _usersRepository.GetAsync(request.Id, cancellationToken);
		}
	}
}