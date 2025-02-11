﻿using Serilog.Context;

namespace BookingService.API.Middlewares;

public class RequestLogContextMiddleware
{
	private readonly RequestDelegate _next;

	public RequestLogContextMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public Task InvokeAsync(HttpContext httpContext)
	{
		using (LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier))
		{
			return _next(httpContext);
		}
	}
}