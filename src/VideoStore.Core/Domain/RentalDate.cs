using System;

namespace VideoStore.Core.Domain
{
    public class RentalDate : BaseDate
    {
        protected RentalDate() { }

        public RentalDate(string date)
        {
            if (!Validate(date)) throw new DomainException("Invalid rental date.");
            Date = DateTime.Parse(date);
        }

        public override bool Validate(string date)
        {
            if (!base.Validate(date)) return false;

            return true;
        }
    }
}
