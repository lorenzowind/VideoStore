using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> AddMoviesRange(IFormFile csvFile);
        Task<PagedResult<Movie>> GetAllMovies(int ps = 8, int page = 1, string q = null);
    }

    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IRentalRepository _rentalRepository;

        public MovieService(IMovieRepository movieRepository, IRentalRepository rentalRepository)
        {
            _movieRepository = movieRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<List<Movie>> AddMoviesRange(IFormFile csvFile)
        {
            var movies = await GetMoviesFromFile(csvFile);

            foreach (var movie in movies)
            {
                if (await _movieRepository.GetById(movie.Id) != null)
                    throw new DomainException($"Title '{movie.Title}' has Id '{movie.Id}' duplicated.");
            }

            _movieRepository.AddRange(movies.ToArray());

            await _rentalRepository.UnitOfWork.Commit();

            return movies;
        }

        public async Task<PagedResult<Movie>> GetAllMovies(int ps = 8, int page = 1, string q = null)
            => await _movieRepository.GetAll(ps, page, q);

        private static async Task<List<Movie>> GetMoviesFromFile(IFormFile csvFile)
        {
            var movies = new List<Movie>();

            var filePath = Path.GetRandomFileName();

            try
            {
                using (var stream = File.Create(filePath))
                {
                    await csvFile.CopyToAsync(stream);
                    stream.Flush();
                }

                using var reader = new StreamReader(filePath);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };

                using var csv = new CsvReader(reader, config);

                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var movie = csv.GetRecord<dynamic>();

                    movies.Add(new Movie(int.Parse(movie.Id), movie.Titulo, int.Parse(movie.ClassificacaoIndicativa), int.Parse(movie.Lancamento)));
                }

                reader.Close();

                new FileInfo(filePath).Delete();
            }
            catch (Exception exception)
            {
                new FileInfo(filePath).Delete();

                throw new DomainException("Csv format data is not valid.", exception);
            }

            return movies;
        }
    }
}
