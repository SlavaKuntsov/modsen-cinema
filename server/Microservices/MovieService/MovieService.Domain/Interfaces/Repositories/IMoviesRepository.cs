﻿using MovieService.Domain.Entities;

namespace MovieService.Domain.Interfaces.Repositories;

public interface IMoviesRepository
{
	Task<MovieEntity?> GetAsync(Guid id, CancellationToken cancellationToken);
	Task<GenreEntity?> GetGenreAsync(Guid id, CancellationToken cancellationToken);
	Task<IList<GenreEntity>> GetGenresAsync(CancellationToken cancellationToken);
	Task<Guid> CreateAsync(MovieEntity movie, CancellationToken cancellationToken);
	void Update(MovieEntity movie, CancellationToken cancellationToken);
	void Delete(MovieEntity movie);
	void Update(GenreEntity genre, CancellationToken cancellationToken);
	void Delete(GenreEntity genre);
	IQueryable<MovieEntity> Get();
	Task<IList<MovieEntity>> ToListAsync(IQueryable<MovieEntity> query, CancellationToken cancellationToken);
	IQueryable<MovieEntity> FilterByGenre(IQueryable<MovieEntity> query, string genreFilter);
	Task<GenreEntity?> GetGenreByNameAsync(string name, CancellationToken cancellationToken);
	Task AddGenreAsync(GenreEntity genre, CancellationToken cancellationToken);
}