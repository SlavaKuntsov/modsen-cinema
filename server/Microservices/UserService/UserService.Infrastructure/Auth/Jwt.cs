﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using UserService.Application.Interfaces.Auth;
using UserService.Domain.Enums;
using UserService.Domain.Extensions;
using UserService.Domain.Interfaces.Repositories;

namespace UserService.Infrastructure.Auth;

public class Jwt : IJwt
{
	private readonly JwtModel _jwtOptions;
	private readonly ITokensRepository _tokensRepository;

	public Jwt(IOptions<JwtModel> jwtOptions, ITokensRepository tokensRepository)
	{
		_jwtOptions = jwtOptions.Value;
		_tokensRepository = tokensRepository;
	}

	public string GenerateAccessToken(Guid id, Role role)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, id.ToString()),
			new Claim(ClaimTypes.Role, EnumExtensions.GetDescription(role))
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
			signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		var randomBytes = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomBytes);

		return Convert.ToBase64String(randomBytes);
	}

	public async Task<Guid> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
	{
		var storedToken = await _tokensRepository.GetRefreshTokenAsync(refreshToken, cancellationToken);

		if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
			return Guid.Empty;
		
		return storedToken.UserId.Value;
	}

	public int GetRefreshTokenExpirationDays()
	{
		return _jwtOptions.RefreshTokenExpirationDays;
	}
}