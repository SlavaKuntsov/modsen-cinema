﻿namespace MovieService.Domain.Interfaces.Repositories.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
	Task SaveChangesAsync(CancellationToken cancellationToken);

	public IDaysRepository DaysRepository { get; }
	public IHallsRepository HallsRepository { get; }
	public ISeatsRepository SeatsRepository { get; }
	public IMovieGenresRepository MovieGenresRepository { get; }
	public IMoviesRepository MoviesRepository { get; }
	public ISessionsRepository SessionsRepository { get; }
}
