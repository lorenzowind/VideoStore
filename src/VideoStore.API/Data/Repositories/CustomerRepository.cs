using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Domain;

namespace VideoStore.API.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Customer customer)
            => _context.Customers.Add(customer);

        public void Update(Customer customer) 
            => _context.Customers.Update(customer);

        public void Remove(Customer customer) 
            => _context.Customers.Remove(customer);

        public async Task<Customer> GetById(int id)
            => await _context.Customers.FindAsync(id);

        public async Task<Customer> GetByCpf(string cpf)
            => await _context.Customers.FirstOrDefaultAsync(c => c.Cpf.Number == cpf);

        public async Task<PagedResult<Customer>> GetAll(int pageSize, int pageIndex, string query = null)
        {
            var sql = @$"SELECT * FROM Customers 
                      WHERE (@Name IS NULL OR Name LIKE CONCAT('%', @Name, '%')) 
                      ORDER BY Name
                      LIMIT {pageSize * (pageIndex - 1)}, {pageSize};
                      SELECT COUNT(Id) FROM Customers 
                      WHERE (@Name IS NULL OR Name LIKE CONCAT('%', @Name, '%'));";

            var multi = await _context.Database.GetDbConnection()
                .QueryMultipleAsync(sql, new { Name = query });

            var customers = multi.Read<dynamic>()
                .Select(c => new Customer(c.Id, c.Name, c.CPF, c.BirthDate));

            var total = multi.Read<int>().FirstOrDefault();

            return new PagedResult<Customer>()
            {
                List = customers,
                TotalResults = total,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Query = query
            };
        }

        public IEnumerable<CustomerDto> GetMinData()
            => _context.Customers.Select(c => new CustomerDto(c.Id, c.Name));
    }
}
