using System;
using VideoStore.Core.Protocols;

namespace VideoStore.Core.Domain
{
    public class BirthDate : BaseDate
    {
        protected BirthDate() { }

        public BirthDate(DateTime date) : base(date) { }

        public BirthDate(string date)
        {
            if (!Validate(date)) throw new DomainException($"Birth date '{date}' is not valid.");
        }

        public override bool Validate(string date)
        {
            if (!base.Validate(date)) return false;

            if (IsFuture()) return false;

            return true;
        }
    }
}
