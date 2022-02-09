using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Models
{
    public class Customer : Entity
    {
        public string Name { get; set; }
        public Cpf Cpf { get; private set; }
        public BirthDate BirthDate { get; private set; }

        protected Customer() { }

        public Customer(string name, string cpf, string birthDate)
        {
            Name = name;
            Cpf = new Cpf(cpf);
            BirthDate = new BirthDate(birthDate);
        }

        public Customer(int id, string name, string cpf, DateTime birthDate)
        {
            Id = id;
            Name = name;
            Cpf = new Cpf(cpf);
            BirthDate = new BirthDate(birthDate);
        }

        public void ChangeCpf(string cpf) => Cpf = new Cpf(cpf);

        public void ChangeBirthDate(string birthDate) => BirthDate = new BirthDate(birthDate);
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CustomerDto(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class CustomerViewModel
    {
        [Required(ErrorMessage = "Property {0} is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property {0} is required.")]
        public string Cpf { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Property {0} is required (use format yyyy-MM-dd).")]
        public string BirthDate { get; set; }
    }

    public interface ICustomerRepository
    {
        void Add(Customer customer);
        void Update(Customer customer);
        void Remove(Customer customer);

        Task<Customer> GetById(int id);
        Task<Customer> GetByCpf(string cpf);
        Task<PagedResult<Customer>> GetAll(int pageSize, int pageIndex, string query = null);
        IEnumerable<CustomerDto> GetMinData();
    }
}
