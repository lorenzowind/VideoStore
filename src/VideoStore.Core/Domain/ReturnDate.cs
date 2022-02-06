using System;
using VideoStore.Core.Protocols;

namespace VideoStore.Core.Domain
{
    public class ReturnDate : BaseDate
    {
        protected ReturnDate() { }

        public ReturnDate(DateTime date) : base(date) { }

        public ReturnDate(string date)
        {
            if (!Validate(date)) throw new DomainException($"Return date '{date}' is not valid.");
        }

        public override bool Validate(string date)
        {
            if (!base.Validate(date)) return false;

            return true;
        }
    }
}
