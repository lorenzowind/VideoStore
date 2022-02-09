using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using VideoStore.Core.Data;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Models
{
    public class Rental : Entity, IAggregateRoot
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public RentalDate RentalDate { get; set; }
        public ReturnDate ReturnDate { get; set; }

        protected Rental() { }

        public Rental(int customerId, int movieId, string rentalDate, string returnDate = null)
        {
            CustomerId = customerId;
            MovieId = movieId;
            RentalDate = new RentalDate(rentalDate);
            ReturnDate = returnDate != null ? new ReturnDate(returnDate) : null;
        }

        public Rental(int id, int customerId, int movieId, DateTime rentalDate, DateTime? returnDate)
        {
            Id = id;
            CustomerId = customerId;
            MovieId = movieId;
            RentalDate = new RentalDate(rentalDate);
            ReturnDate = returnDate != null ? new ReturnDate((DateTime)returnDate) : null;
        }

        public void ChangeRentalDate(string rentalDate) => RentalDate = new RentalDate(rentalDate);

        public void ChangeReturnDate(string returnDate) => ReturnDate = new ReturnDate(returnDate);
    }

    public class RentalDto : Rental
    {
        public bool IsLate { get; set; }

        public RentalDto(int id, int customerId, int movieId, int movieLaunch, DateTime rentalDate, DateTime? returnDate)
            : base(id, customerId, movieId, rentalDate, returnDate)
        {
            if (returnDate == null)
            {
                var daysDiff = (DateTime.Now.Date - rentalDate.Date).Days;

                IsLate = daysDiff > (Convert.ToBoolean(movieLaunch) 
                    ? Constants.LAUNCH_MOVIE_DAYS_DEADLINE 
                    : Constants.NON_LAUNCH_MOVIE_DAYS_DEADLINE);
            }
        }
    }

    public class RentalViewModel
    {
        [Required(ErrorMessage = "Property {0} is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Property {0} is required.")]
        public int MovieId { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Property {0} is required (use format yyyy-MM-dd).")]
        public string RentalDate { get; set; }

        [DataType(DataType.Date)]
        public string ReturnDate { get; set; }
    }

    public interface IRentalRepository : IRepository<Rental>
    {
        void Add(Rental rental);
        void Update(Rental rental);
        void Remove(Rental rental);

        Task<Rental> GetById(int id);
        Task<Rental> GetRentedByMovieId(int movieId);
        Task<PagedResult<RentalDto>> GetAll(int pageSize, int pageIndex, DateTime? query);

        Task<List<Customer>> GetLateReturnCustomers();
        Task<Customer> GetMoreRentsCustomerByPosition(int position);

        Task<List<Movie>> GetNeverRentedMovies();
        Task<List<Movie>> GetMostRentedMoviesInLastYear(int quantity);
        Task<List<Movie>> GetLessRentedMoviesInLastWeek(int quantity);
    }
}
