using System;

namespace VideoStore.Core.Domain
{
    public abstract class BaseDate
    {
        public DateTime Date { get; protected set; }

        public virtual bool Validate(string date)
        {
            try 
            {
                DateTime.Parse(date);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
