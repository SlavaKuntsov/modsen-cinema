﻿using MovieService.Application.Handlers.Commands.Sessoins.FillSession;

using Swashbuckle.AspNetCore.Filters;

namespace MovieService.API.Contracts.RequestExamples.Sessions;

public class FillSessionRequestExample : IExamplesProvider<FillSessionCommand>
{
	public FillSessionCommand GetExamples()
	{
		return new FillSessionCommand(
			Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
			Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
			"05-01-2025 09:00");
	}
}