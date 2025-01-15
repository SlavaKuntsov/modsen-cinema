﻿using MovieService.Application.Handlers.Commands.Halls.CreateHall;

using Swashbuckle.AspNetCore.Filters;

namespace MovieService.API.Contracts;

public class CreateCustomHallRequestExample : IExamplesProvider<CreateCustomHallCommand>
{
	public CreateCustomHallCommand GetExamples()
	{
		return new CreateCustomHallCommand(
			"Большой зал",
			14,
			[
				[1, 1, 1, 1],
				[2, 3, 3, 2],
				[0, 0, 0, 0],
				[-1, 0, 0, -1]
			]);
	}
}