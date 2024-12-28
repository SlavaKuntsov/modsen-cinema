using System.Security.Claims;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Filters;

using UserService.API.Contracts;
using UserService.API.Contracts.Examples;
using UserService.Application.Handlers.Commands.Tokens;
using UserService.Application.Handlers.Commands.Users;
using UserService.Application.Handlers.Queries.Users;

namespace UserService.API.Controllers.Http;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IMapper _mapper;

	public UserController(IMediator mediator, IMapper mapper)
	{
		_mediator = mediator;
		_mapper = mapper;
	}

	[HttpPost(nameof(Login))]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[SwaggerRequestExample(typeof(CreateLoginRequest), typeof(CreateLoginRequestExample))]
	public async Task<IActionResult> Login([FromBody] CreateLoginRequest request)
	{
		var existUser = await _mediator.Send(new LoginQuery(request.Email, request.Password));

		var authResultDto = await _mediator.Send(new GenerateAndUpdateTokensCommand(existUser.Id, existUser.Role));

		HttpContext.Response.Cookies.Append(Domain.Constants.JwtConstants.COOKIE_NAME, authResultDto.RefreshToken);

		return Ok(authResultDto);
	}

	[HttpPost(nameof(Registration))]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[SwaggerRequestExample(typeof(CreateUserRequest), typeof(CreateUserRequestExample))]
	public async Task<IActionResult> Registration([FromBody] CreateUserRequest request)
	{
		var authResultDto = await _mediator.Send(new UserRegistrationCommand(
			request.Email,
			request.Password,
			request.Firstname,
			request.Lastname,
			request.DateOfBirth));

		return Ok(authResultDto);
	}

	[HttpPut(nameof(Update))]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[SwaggerRequestExample(typeof(UpdateUserRequest), typeof(UpdateUserRequestExample))]
	[Authorize(Policy = "UserOrAdmin")]
	public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
	{
		var particantModel = await _mediator.Send(new UpdateUserCommand(
			request.Id,
			request.Firstname,
			request.Lastname,
			request.DateOfBirth));

		return Ok(particantModel);
	}

	[HttpDelete(nameof(Delete) + "/{id:Guid}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[Authorize(Policy = "UserOrAdmin")]
	public async Task<IActionResult> Delete([FromRoute] Guid id)
	{
		var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

		if (userIdClaim == null)
			throw new UnauthorizedAccessException("User ID not found in claims.");

		if (Guid.Parse(userIdClaim.Value.ToString()) != id)
			throw new UnauthorizedAccessException("User cannot delete another User.");

		await _mediator.Send(new DeleteUserCommand(id));
		return Ok();
	}

	[HttpGet(nameof(Users))]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[Authorize(Policy = "AdminOnly")]
	public async Task<IActionResult> Users()
	{
		var users = await _mediator.Send(new GetAllUsersQuery());

		return Ok(_mapper.Map<IList<UserDto>>(users));
	}
}