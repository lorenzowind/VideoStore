using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Domain;

namespace VideoStore.API.Data.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly ApplicationDbContext _context;

        public MovieRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddRange(Movie[] movies)
            => _context.Movies.AddRange(movies);

        public async Task<Movie> GetById(int id)
            => await _context.Movies.FindAsync(id);

        public async Task<PagedResult<Movie>> GetAll(int pageSize, int pageIndex, string query = null)
        {
            var sql = @$"SELECT * FROM Movies 
                      WHERE (@Title IS NULL OR Title LIKE CONCAT('%', @Title, '%')) 
                      ORDER BY Title
                      LIMIT {pageSize * (pageIndex - 1)}, {pageSize};
                      SELECT COUNT(Id) FROM Movies 
                      WHERE (@Title IS NULL OR Title LIKE CONCAT('%', @Title, '%'));";

            var multi = await _context.Database.GetDbConnection()
                .QueryMultipleAsync(sql, new { Title = query });

            var movies = multi.Read<Movie>();

            var total = multi.Read<int>().FirstOrDefault();

            return new PagedResult<Movie>()
            {
                List = movies,
                TotalResults = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Query = query
            };
        }
    }
}
