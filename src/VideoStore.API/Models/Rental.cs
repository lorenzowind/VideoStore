using VideoStore.Core.Domain;

namespace VideoStore.API.Models
{
    public class Rental : Entity
    {
        public int CustomerId { get; private set; }
        public Customer Customer { get; private set; }
        public int MovieId { get; private set; }
        public Movie Movie { get; private set; }
        public RentalDate RentalDate { get; private set; }
        public ReturnDate ReturnDate { get; private set; }

        protected Rental() { }

        public Rental(int customerId, int movieId, string rentalDate, string returnDate)
        {
            CustomerId = customerId;
            MovieId = movieId;
            RentalDate = new RentalDate(rentalDate);
            ReturnDate = new ReturnDate(returnDate);
        }
    }
}
