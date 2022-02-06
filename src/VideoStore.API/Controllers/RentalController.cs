using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.API.Services;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Controllers
{
    [Route("api/rental")]
    public class RentalController : MainController
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(RentalViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            try
            {
                return CustomResponse(await _rentalService.AddRental(model));
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Save(int id, RentalViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            try
            {
                return CustomResponse(await _rentalService.UpdateRental(id, model));
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
                await _rentalService.RemoveRental(id);
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }

        [HttpGet("all")]
        public async Task<PagedResult<Rental>> Index([FromQuery] int ps = 8, [FromQuery] int page = 1, [FromQuery] string q = null)
        {
            return await _rentalService.GetAllRentals(ps, page, q);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportReport()
        {
            try
            {
                var stream = await _rentalService.GenerateReport();

                string fileName = $"VideoStoreReport-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";

                return File(stream, "application/octet-stream", fileName);
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }
    }
}
