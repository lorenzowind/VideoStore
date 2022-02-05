using System;

namespace VideoStore.Core.Domain
{
    public class ReturnDate : BaseDate
    {
        protected ReturnDate() { }

        public ReturnDate(string date)
        {
            if (!Validate(date)) throw new DomainException("Invalid return date.");
            Date = DateTime.Parse(date);
        }

        public override bool Validate(string date)
        {
            if (!base.Validate(date)) return false;

            return true;
        }
    }
}
