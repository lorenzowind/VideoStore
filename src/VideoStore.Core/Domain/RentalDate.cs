using System;
using VideoStore.Core.Protocols;

namespace VideoStore.Core.Domain
{
    public class RentalDate : BaseDate
    {
        protected RentalDate() { }

        public RentalDate(DateTime date) : base(date) { }

        public RentalDate(string date)
        {
            if (!Validate(date)) throw new DomainException($"Rental date '{date}' is not valid.");
        }

        public override bool Validate(string date)
        {
            if (!base.Validate(date)) return false;

            return true;
        }
    }
}
