using VideoStore.Core.Domain;

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
    }
}
