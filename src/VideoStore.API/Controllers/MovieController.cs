using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.API.Services;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Controllers
{
    [Route("api/movie")]
    public class MovieController : MainController
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> CreateRange(IFormFile csvFile)
        {
            try
            {
                return CustomResponse(await _movieService.AddMoviesRange(csvFile));
            }
            catch (DomainException exception)
            {
                AddProcessingError(exception.Message);
            }

            return CustomResponse();
        }

        [HttpGet("all")]
        public async Task<PagedResult<Movie>> Index([FromQuery] int ps = 8, [FromQuery] int page = 1, [FromQuery] string q = null)
        {
            return await _movieService.GetAllMovies(ps, page, q);
        }

        [HttpGet("min-data")]
        public IEnumerable<MovieDto> Get()
        {
            return _movieService.GetMoviesMinData();
        }
    }
}
