﻿using System.Diagnostics;
using System.Reflection;
using System.Text;

using BookingService.API.Behaviors;
using BookingService.API.ExceptionHandlers;
using BookingService.Domain.Constants;
using BookingService.Infrastructure.Auth;

using Mapster;

using MapsterMapper;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Protobufs.Auth;

using Swashbuckle.AspNetCore.Filters;

namespace BookingService.API.Extensions;

public static class ApiExtensions
{
	public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddExceptionHandler<GlobalExceptionHandler>();
		services.AddProblemDetails();
		services.AddHealthChecks();

		var usersPort = Environment.GetEnvironmentVariable("USERS_APP_PORT");

		if (string.IsNullOrEmpty(usersPort))
			usersPort = configuration.GetValue<string>("ApplicationSettings:UsersPort");

		services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
		{
			options.Address = new Uri($"https://localhost:{usersPort}");
		});

		services.AddHttpContextAccessor();

		services.AddControllers()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(
					new Infrastructure.DateTime.DateTimeConverter(DateTimeConstants.DATE_TIME_FORMAT));
			});
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.ExampleFilters();
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
			{
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				Description = "Введите токен JWT в формате 'Bearer {токен}'"
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});
		});

		TypeAdapterConfig typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
		typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());

		Mapper mapperConfig = new(typeAdapterConfig);
		services.AddSingleton<IMapper>(mapperConfig);

		JwtModel? jwtOptions = configuration.GetSection(nameof(JwtModel)).Get<JwtModel>();

		services
			.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				options.RequireHttpsMetadata = true;
				options.SaveToken = true;
				options.TokenValidationParameters = new()
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(jwtOptions!.SecretKey))
				};
				options.Events = new JwtBearerEvents
				{
					OnAuthenticationFailed = context =>
					{
						Debug.WriteLine(context.Request.Headers.Authorization);
						Debug.WriteLine("Authentication failed: " + context.Exception.Message);
						return Task.CompletedTask;
					},
					OnTokenValidated = context =>
					{
						Debug.WriteLine("Token is valid.");
						return Task.CompletedTask;
					}
				};
			});

		services.Configure<JwtModel>(configuration.GetSection(nameof(JwtModel)));
		services.Configure<AuthorizationOptions>(configuration.GetSection(nameof(AuthorizationOptions)));

		services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.AllowAnyHeader();
				policy.AllowAnyMethod();
				policy.AllowCredentials();
			});
		});

		services.AddAuthorizationBuilder()
			.AddPolicy("AdminOnly", policy =>
			{
				policy.RequireRole("Admin");
				policy.AddRequirements(new ActiveAdminRequirement());
			})
			.AddPolicy("UserOnly", policy => policy.RequireRole("User"))
			.AddPolicy("UserOrAdmin", policy =>
			{
				policy.RequireRole("User", "Admin");
				policy.AddRequirements(new ActiveAdminRequirement());
			});

		services.AddScoped<IAuthorizationHandler, ActiveAdminHandler>();

		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

		return services;
	}

	public static void UseHttps(this WebApplicationBuilder builder)
	{
		var environment = builder.Environment;

		var portString = Environment.GetEnvironmentVariable("PORT");

		if (string.IsNullOrEmpty(portString))
			portString = builder.Configuration.GetValue<string>("ApplicationSettings:Port");

		if (!int.TryParse(portString, out int port))
			throw new InvalidOperationException($"Invalid port value: {portString}");

		if (environment.IsProduction())
		{
			var certPath = "/app/localhost.pfx";
			var certPassword = "1";
			builder.WebHost.ConfigureKestrel(options =>
			{
				options.ListenAnyIP(port, listenOptions =>
				{
					listenOptions.UseHttps(certPath, certPassword);
				});
			});
		}
		else
		{
			builder.WebHost.ConfigureKestrel(options =>
			{
				options.ListenAnyIP(port, listenOptions =>
				{
					listenOptions.UseHttps();
				});
			});
		}
	}
}