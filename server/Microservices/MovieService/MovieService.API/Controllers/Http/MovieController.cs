﻿using MediatR;

using Microsoft.AspNetCore.Mvc;

using MovieService.API.Contracts.Examples.Movies;
using MovieService.API.Contracts.RequestExamples.Movies;
using MovieService.API.Contracts.Requests;
using MovieService.Application.Handlers.Commands.Movies.CreateMovie;
using MovieService.Application.Handlers.Commands.Movies.DeleteGenre;
using MovieService.Application.Handlers.Commands.Movies.DeleteMovie;
using MovieService.Application.Handlers.Commands.Movies.UpdateGenre;
using MovieService.Application.Handlers.Commands.Movies.UpdateMovie;
using MovieService.Application.Handlers.Queries.Movies.GetAllGenres;
using MovieService.Application.Handlers.Queries.Movies.GetAllMovies;
using MovieService.Application.Handlers.Queries.Movies.GetMovieById;
using MovieService.Domain.Exceptions;

using Swashbuckle.AspNetCore.Filters;

namespace MovieService.API.Controllers.Http;

[ApiController]
[Route("[controller]")]
public class MovieController : ControllerBase
{
	private readonly IMediator _mediator;

	public MovieController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet("/Movies")]
	public async Task<IActionResult> Get([FromQuery] GetMovieRequest request)
	{
		var movies = await _mediator.Send(new GetAllMoviesQuery(
			request.Limit,
			request.Offset,
			request.Filter,
			request.FilterValue,
			request.SortBy,
			request.SortDirection));

		return Ok(movies);
	}

	[HttpGet("/Movies/{id:Guid}")]
	public async Task<IActionResult> Get([FromRoute] Guid id)
	{
		var movies = await _mediator.Send(new GetMovieByIdQuery(id))
			?? throw new NotFoundException($"Movie with id '{id.ToString()}' not found.");

		return Ok(movies);
	}

	[HttpPost("/Movies")]
	[SwaggerRequestExample(typeof(CreateMovieCommand), typeof(CreateMovieRequestExample))]
	//[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Create([FromBody] CreateMovieCommand request)
	{
		var movie = await _mediator.Send(request);

		return Ok(movie);
	}

	[HttpPatch("/Movies")]
	[SwaggerRequestExample(typeof(UpdateMovieCommand), typeof(UpdateMovieRequestExample))]
	//[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Update([FromBody] UpdateMovieCommand request)
	{
		var movie = await _mediator.Send(request);

		return Ok(movie);
	}

	[HttpDelete("/Movies/{id:Guid}")]
	//[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Delete([FromRoute] Guid id)
	{
		await _mediator.Send(new DeleteMovieCommand(id));

		return NoContent();
	}

	[HttpGet("/Movies/Genres")]
	public async Task<IActionResult> Get()
	{
		var genres = await _mediator.Send(new GetAllGenresQuery());

		return Ok(genres);
	}

	[HttpPatch("/Movies/Genres")]
	[SwaggerRequestExample(typeof(UpdateGenreCommand), typeof(UpdateGenreCommandExample))]
	//[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Update([FromBody] UpdateGenreCommand request)
	{
		var genre = await _mediator.Send(request);

		return Ok(genre);
	}

	[HttpDelete("/Movies/Genres/{id:Guid}")]
	//[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> DeleteGenre([FromRoute] Guid id)
	{
		await _mediator.Send(new DeleteGenreCommand(id));

		return NoContent();
	}
}