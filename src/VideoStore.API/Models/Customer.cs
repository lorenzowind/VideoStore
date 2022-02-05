using VideoStore.Core.Domain;

namespace VideoStore.API.Models
{
    public class Customer : Entity
    {
        public string Name { get; private set; }
        public Cpf Cpf { get; private set; }
        public BirthDate BirthDate { get; private set; }

        protected Customer() { }

        public Customer(string name, string cpf, string birthDate)
        {
            Name = name;
            Cpf = new Cpf(cpf);
            BirthDate = new BirthDate(birthDate);
        }
    }
}
