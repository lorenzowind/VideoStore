using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Data;
using VideoStore.Core.Domain;

namespace VideoStore.API.Data.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext _context;

        public RentalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUnitOfWork UnitOfWork => _context;

        public void Add(Rental rental)
            => _context.Rentals.Add(rental);

        public void Update(Rental rental)
            => _context.Rentals.Update(rental);

        public void Remove(Rental rental)
            => _context.Rentals.Remove(rental);

        public async Task<Rental> GetById(int id)
            => await _context.Rentals.FindAsync(id);

        public async Task<Rental> GetRentedByMovieId(int movieId)
            => await _context.Rentals.FirstOrDefaultAsync(r => r.MovieId == movieId && r.ReturnDate == null);

        public async Task<PagedResult<RentalDto>> GetAll(int pageSize, int pageIndex, DateTime? query)
        {
            var sql = @$"SELECT 
                      R.Id AS Id, R.RentalDate AS RentalDate, R.ReturnDate AS ReturnDate,
                      C.Id AS CustomerId, C.Name AS CustomerName, C.CPF AS CustomerCPF, C.BirthDate AS CustomerBirthDate,
                      M.Id AS MovieId, M.Title AS MovieTitle, M.ParentalRating AS MovieParentalRating, M.Launch AS MovieLaunch
                      FROM Rentals R
                      INNER JOIN Customers C ON R.CustomerId = C.Id
                      INNER JOIN Movies M ON R.MovieId = M.Id
                      WHERE (@RentalDate IS NULL OR RentalDate = @RentalDate) 
                      ORDER BY RentalDate
                      LIMIT {pageSize * (pageIndex - 1)}, {pageSize};
                      SELECT COUNT(Id) FROM Rentals 
                      WHERE (@RentalDate IS NULL OR RentalDate = @RentalDate);";

            var multi = await _context.Database.GetDbConnection()
                .QueryMultipleAsync(sql, new { RentalDate = query });

            var rentals = multi.Read<dynamic>()
                .Select(r => new RentalDto(r.Id, r.CustomerId, r.MovieId, r.MovieLaunch, r.RentalDate, r.ReturnDate) 
                    { 
                        Customer = new Customer(r.CustomerId, r.CustomerName, r.CustomerCPF, r.CustomerBirthDate), 
                        Movie = new Movie(r.MovieId, r.MovieTitle, r.MovieParentalRating, r.MovieLaunch)
                    });

            var total = multi.Read<int>().FirstOrDefault();

            return new PagedResult<RentalDto>()
            {
                List = rentals,
                TotalResults = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Query = query.ToString()
            };
        }

        public async Task<List<Customer>> GetLateReturnCustomers()
        {
            var sql = @$"SELECT
                      C.Id AS CustomerId, C.Name AS CustomerName, C.CPF AS CustomerCPF, C.BirthDate AS CustomerBirthDate
                      FROM Rentals R
                      INNER JOIN Customers C ON R.CustomerId = C.Id
                      INNER JOIN Movies M ON R.MovieId = M.Id
                      WHERE (M.Launch = 1 AND ReturnDate IS NULL AND datediff(RentalDate, current_date()) > @LAUNCH_MOVIE_DAYS_DEADLINE)
                      OR (M.Launch = 0 AND ReturnDate IS NULL AND datediff(RentalDate, current_date()) > @NON_LAUNCH_MOVIE_DAYS_DEADLINE) 
                      ORDER BY RentalDate;";

            var customers = await _context.Database.GetDbConnection().QueryAsync<dynamic>(sql, new 
            {
                Constants.LAUNCH_MOVIE_DAYS_DEADLINE,
                Constants.NON_LAUNCH_MOVIE_DAYS_DEADLINE,
            });

            return customers.Select(r => new Customer(r.CustomerId, r.CustomerName, r.CustomerCPF, r.CustomerBirthDate)).ToList();
        }

        public async Task<Customer> GetMoreRentsCustomerByPosition(int position)
        {
            var sql = @$"SELECT
                      C.Id AS CustomerId, C.Name AS CustomerName, C.CPF AS CustomerCPF, C.BirthDate AS CustomerBirthDate,
                      COUNT(*) AS RentedMovies
                      FROM Rentals R
                      INNER JOIN Customers C ON R.CustomerId = C.Id
                      GROUP BY CustomerId
                      ORDER BY RentedMovies DESC
                      LIMIT @Position, 1;";

            var customers = await _context.Database.GetDbConnection().QueryAsync<dynamic>(sql, new { Position = position - 1 });

            return customers.Select(r => new Customer(r.CustomerId, r.CustomerName, r.CustomerCPF, r.CustomerBirthDate)).FirstOrDefault();
        }

        public async Task<List<Movie>> GetNeverRentedMovies()
        {
            var sql = @$"SELECT
                      M.Id AS MovieId, M.Title AS MovieTitle, M.ParentalRating AS MovieParentalRating, M.Launch AS MovieLaunch
                      FROM Movies M
                      WHERE NOT EXISTS(
                      SELECT * FROM Rentals R 
                      WHERE R.MovieId = M.Id)
                      ORDER BY M.Id;";

            var movies = await _context.Database.GetDbConnection().QueryAsync<dynamic>(sql);

            return movies.Select(r => new Movie(r.MovieId, r.MovieTitle, r.MovieParentalRating, r.MovieLaunch)).ToList();
        }

        public async Task<List<Movie>> GetMostRentedMoviesInLastYear(int quantity)
        {
            var sql = @$"SELECT
                      M.Id AS MovieId, M.Title AS MovieTitle, M.ParentalRating AS MovieParentalRating, M.Launch AS MovieLaunch,
                      COUNT(*) AS RentedTimes
                      FROM Movies M
                      INNER JOIN Rentals R ON R.MovieId = M.Id
                      WHERE YEAR(R.RentalDate) = YEAR(current_date())
                      GROUP BY M.Id
                      ORDER BY RentedTimes DESC
                      LIMIT @Quantity;";

            var movies = await _context.Database.GetDbConnection().QueryAsync<dynamic>(sql, new { Quantity = quantity });

            return movies.Select(r => new Movie(r.MovieId, r.MovieTitle, r.MovieParentalRating, r.MovieLaunch)).ToList();
        }

        public async Task<List<Movie>> GetLessRentedMoviesInLastWeek(int quantity)
        {
            var sql = @$"SELECT
                      M.Id AS MovieId, M.Title AS MovieTitle, M.ParentalRating AS MovieParentalRating, M.Launch AS MovieLaunch,
                      COUNT(*) AS RentedTimes
                      FROM Movies M
                      INNER JOIN Rentals R ON R.MovieId = M.Id
                      WHERE datediff(RentalDate, current_date()) <= 7
                      GROUP BY M.Id
                      ORDER BY RentedTimes ASC
                      LIMIT @Quantity;";

            var movies = await _context.Database.GetDbConnection().QueryAsync<dynamic>(sql, new { Quantity = quantity });

            return movies.Select(r => new Movie(r.MovieId, r.MovieTitle, r.MovieParentalRating, r.MovieLaunch)).ToList();
        }

        public void Dispose() => _context.Dispose();
    }
}
