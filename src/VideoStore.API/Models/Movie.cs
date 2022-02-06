using System.Threading.Tasks;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Models
{
    public class Movie : Entity
    {
        public string Title { get; private set; }
        public int ParentalRating { get; private set; }
        public int Launch { get; private set; }

        protected Movie() { }

        public Movie(string title, int parentalRating, int launch)
        {
            Title = title;
            ParentalRating = parentalRating;
            Launch = launch;
        }

        public Movie(int id, string title, int parentalRating, int launch)
        {
            Id = id;
            Title = title;
            ParentalRating = parentalRating;
            Launch = launch;
        }
    }

    public class MovieCsvRow
    {
        public int Id { get; private set; }
        public string Titulo { get; private set; }
        public int ClassificacaoIndicativa { get; private set; }
        public int Lancamento { get; private set; }
    }

    public interface IMovieRepository
    {
        void AddRange(Movie[] movies);

        Task<Movie> GetById(int id);
        Task<PagedResult<Movie>> GetAll(int pageSize, int pageIndex, string query = null);
    }
}
