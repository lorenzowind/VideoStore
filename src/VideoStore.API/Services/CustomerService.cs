using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Services
{
    public interface ICustomerService
    {
        Task<Customer> AddCustomer(CustomerViewModel model);
        Task<Customer> UpdateCustomer(int id, CustomerViewModel model);
        Task RemoveCustomer(int id);
        Task<PagedResult<Customer>> GetAllCustomers(int ps = 8, int page = 1, string q = null);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IRentalRepository _rentalRepository;

        public CustomerService(ICustomerRepository customerRepository, IRentalRepository rentalRepository)
        {
            _customerRepository = customerRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<Customer> AddCustomer(CustomerViewModel model)
        {
            if (await _customerRepository.GetByCpf(model.Cpf) != null)
                throw new DomainException($"Cpf '{model.Cpf}' already taken.");

            var customer = new Customer(model.Name, model.Cpf, model.BirthDate);

            _customerRepository.Add(customer);

            await _rentalRepository.UnitOfWork.Commit();

            return customer;
        }

        public async Task<Customer> UpdateCustomer(int id, CustomerViewModel model)
        {
            var customer = await _customerRepository.GetById(id);

            if (customer == null) throw new DomainException($"Customer with Id '{id}' not found.");

            var existingCustomer = await _customerRepository.GetByCpf(model.Cpf);

            if (existingCustomer != null && existingCustomer.Id != id)
                throw new DomainException($"Cpf '{model.Cpf}' already taken.");

            customer.Name = model.Name;
            customer.ChangeCpf(model.Cpf);
            customer.ChangeBirthDate(model.BirthDate);

            _customerRepository.Update(customer);

            await _rentalRepository.UnitOfWork.Commit();

            return customer;
        }

        public async Task RemoveCustomer(int id)
        {
            var customer = await _customerRepository.GetById(id);

            if (customer == null) throw new DomainException($"Customer with Id '{id}' not found.");

            _customerRepository.Remove(customer);

            await _rentalRepository.UnitOfWork.Commit();
        }

        public async Task<PagedResult<Customer>> GetAllCustomers(int ps = 8, int page = 1, string q = null)
            => await _customerRepository.GetAll(ps, page, q);
    }
}
