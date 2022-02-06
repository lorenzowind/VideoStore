using System;

namespace VideoStore.Core.Domain
{
    public abstract class BaseDate
    {
        public DateTime Date { get; private set; }

        protected BaseDate() { }

        protected BaseDate(DateTime date)
        {
            Date = date;
        }

        public virtual bool Validate(string date)
        {
            try 
            {
                Date = DateTime.Parse(date);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public int CalculateDaysDif(DateTime endDate) => (endDate - Date).Days;

        public bool IsFuture() => Date > DateTime.Now.Date;
    }
}
