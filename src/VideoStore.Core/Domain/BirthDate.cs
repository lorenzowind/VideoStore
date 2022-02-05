using System;

namespace VideoStore.Core.Domain
{
    public class BirthDate : BaseDate
    {
        protected BirthDate() { }

        public BirthDate(string date)
        {
            if (!Validate(date)) throw new DomainException("Invalid birth date.");
            Date = DateTime.Parse(date);
        }

        public override bool Validate(string date)
        {
            if (!base.Validate(date)) return false;

            return true;
        }
    }
}
