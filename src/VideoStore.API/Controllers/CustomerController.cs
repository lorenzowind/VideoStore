using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.API.Services;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Controllers
{
    [Route("api/customer")]
    public class CustomerController : MainController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            try
            {
                return CustomResponse(await _customerService.AddCustomer(model));
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Save(int id, CustomerViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            try
            {
                return CustomResponse(await _customerService.UpdateCustomer(id, model));
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _customerService.RemoveCustomer(id);
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }

        [HttpGet("all")]
        public async Task<PagedResult<Customer>> Index([FromQuery] int ps = 8, [FromQuery] int page = 1, [FromQuery] string q = null)
        {
            return await _customerService.GetAllCustomers(ps, page, q);
        }

        [HttpGet("min-data")]
        public IEnumerable<CustomerDto> Get()
        {
            return _customerService.GetCustomersMinData();
        }
    }
}
