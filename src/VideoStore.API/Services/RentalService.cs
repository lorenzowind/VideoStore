using OfficeOpenXml;
using System;
using System.IO;
using System.Threading.Tasks;
using VideoStore.API.Models;
using VideoStore.Core.Domain;
using VideoStore.Core.Protocols;

namespace VideoStore.API.Services
{
    public interface IRentalService
    {
        Task<Rental> AddRental(RentalViewModel model);
        Task<Rental> UpdateRental(int id, RentalViewModel model);
        Task RemoveRental(int id);
        Task<PagedResult<RentalDto>> GetAllRentals(int ps = 8, int page = 1, DateTime? q = null);
        Task<MemoryStream> GenerateReport();
    }

    public class RentalService : IRentalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IRentalRepository _rentalRepository;

        public RentalService(
            ICustomerRepository customerRepository,
            IMovieRepository movieRepository,
            IRentalRepository rentalRepository)
        {
            _customerRepository = customerRepository;
            _movieRepository = movieRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<Rental> AddRental(RentalViewModel model)
        {
            if (await _customerRepository.GetById(model.CustomerId) == null)
                throw new DomainException($"Customer with Id '{model.CustomerId}' not found.");

            if (await _movieRepository.GetById(model.MovieId) == null)
                throw new DomainException($"Movie with Id '{model.MovieId}' not found.");

            if (await _rentalRepository.GetRentedByMovieId(model.MovieId) != null)
                throw new DomainException($"Movie with Id '{model.MovieId}' is already rented.");

            var rental = new Rental(model.CustomerId, model.MovieId, model.RentalDate, model.ReturnDate);

            if (rental.ReturnDate != null)
            {
                if (!rental.ReturnDate.IsFuture() && rental.RentalDate.CalculateDaysDif(rental.ReturnDate.Date) < 0)
                    throw new DomainException("Rental date should be greater or equal than return date.");

                if (rental.ReturnDate.IsFuture()) rental.ReturnDate = null;
            }

            _rentalRepository.Add(rental);

            await _rentalRepository.UnitOfWork.Commit();

            return rental;
        }

        public async Task<Rental> UpdateRental(int id, RentalViewModel model)
        {
            var rental = await _rentalRepository.GetById(id);

            if (rental == null) throw new DomainException($"Rental with Id '{id}' not found.");

            if (await _customerRepository.GetById(model.CustomerId) == null)
                throw new DomainException($"Customer with Id '{model.CustomerId}' not found.");

            if (await _movieRepository.GetById(model.MovieId) == null)
                throw new DomainException($"Movie with Id '{model.MovieId}' not found.");

            var rented = await _rentalRepository.GetRentedByMovieId(model.MovieId);

            if (rented != null && rented.Id != id)
                throw new DomainException($"Movie with Id '{model.MovieId}' is already rented.");

            rental.CustomerId = model.CustomerId;
            rental.MovieId = model.MovieId;
            rental.ChangeRentalDate(model.RentalDate);

            if (model.ReturnDate != null)
            {
                rental.ChangeReturnDate(model.ReturnDate);

                if (rental.RentalDate.CalculateDaysDif(rental.ReturnDate.Date) < 0)
                    throw new DomainException($"Rental date should be greater or equal than return date.");

                if (rental.ReturnDate.IsFuture()) rental.ReturnDate = null;
            }
            else rental.ReturnDate = null;

            _rentalRepository.Update(rental);

            await _rentalRepository.UnitOfWork.Commit();

            return rental;
        }

        public async Task RemoveRental(int id)
        {
            var rental = await _rentalRepository.GetById(id);

            if (rental == null) throw new DomainException($"Rental with Id '{id}' not found.");

            _rentalRepository.Remove(rental);

            await _rentalRepository.UnitOfWork.Commit();
        }

        public async Task<PagedResult<RentalDto>> GetAllRentals(int ps = 8, int page = 1, DateTime? q = null)
            => await _rentalRepository.GetAll(ps, page, q);

        public async Task<MemoryStream> GenerateReport()
        {
            var lateReturnCustomers = await _rentalRepository.GetLateReturnCustomers();
            var moreRentsCustomer = await _rentalRepository.GetMoreRentsCustomerByPosition(2);
            var neverRentedMovies = await _rentalRepository.GetNeverRentedMovies();
            var mostRentedMoviesInLastYear = await _rentalRepository.GetMostRentedMoviesInLastYear(5);
            var lessRentedMoviesInLastWeek = await _rentalRepository.GetLessRentedMoviesInLastWeek(3);

            var stream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);

            #region Report_lateReturnCustomers

            var workSheet = package.Workbook.Worksheets.Add("Report-Clientes_em_atraso_na_devolucao");

            workSheet.Cells["A1"].Value = "Id";
            workSheet.Cells["B1"].Value = "Nome";
            workSheet.Cells["C1"].Value = "CPF";
            workSheet.Cells["D1"].Value = "Data de Nascimento";

            var row = 2;

            foreach (var customer in lateReturnCustomers)
            {
                workSheet.Cells[string.Format("A{0}", row)].Value = customer.Id.ToString();
                workSheet.Cells[string.Format("B{0}", row)].Value = customer.Name;
                workSheet.Cells[string.Format("C{0}", row)].Value = customer.Cpf.Number;
                workSheet.Cells[string.Format("D{0}", row)].Value = customer.BirthDate.Date.ToString();
                row++;
            }

            #endregion

            #region Report_neverRentedMovies

            workSheet = package.Workbook.Worksheets.Add("Report-Filmes_que_nunca_foram_alugados");

            workSheet.Cells["A1"].Value = "Id";
            workSheet.Cells["B1"].Value = "Titulo";
            workSheet.Cells["C1"].Value = "Classificacao Indicativa";
            workSheet.Cells["D1"].Value = "Lancamento";

            row = 2;

            foreach (var movie in neverRentedMovies)
            {
                workSheet.Cells[string.Format("A{0}", row)].Value = movie.Id.ToString();
                workSheet.Cells[string.Format("B{0}", row)].Value = movie.Title;
                workSheet.Cells[string.Format("C{0}", row)].Value = movie.ParentalRating.ToString();
                workSheet.Cells[string.Format("D{0}", row)].Value = movie.Launch.ToString();
                row++;
            }

            #endregion

            #region Report_mostRentedMoviesInLastYear

            workSheet = package.Workbook.Worksheets.Add("Report-Cinco_filmes_mais_alugados_do_ultimo_ano");

            workSheet.Cells["A1"].Value = "Id";
            workSheet.Cells["B1"].Value = "Titulo";
            workSheet.Cells["C1"].Value = "Classificacao Indicativa";
            workSheet.Cells["D1"].Value = "Lancamento";

            row = 2;

            foreach (var movie in mostRentedMoviesInLastYear)
            {
                workSheet.Cells[string.Format("A{0}", row)].Value = movie.Id.ToString();
                workSheet.Cells[string.Format("B{0}", row)].Value = movie.Title;
                workSheet.Cells[string.Format("C{0}", row)].Value = movie.ParentalRating.ToString();
                workSheet.Cells[string.Format("D{0}", row)].Value = movie.Launch.ToString();
                row++;
            }

            #endregion

            #region Report_lessRentedMoviesInLastWeek

            workSheet = package.Workbook.Worksheets.Add("Report-Tres_filmes_menos_alugados_da_ultima_semana");

            workSheet.Cells["A1"].Value = "Id";
            workSheet.Cells["B1"].Value = "Titulo";
            workSheet.Cells["C1"].Value = "Classificacao Indicativa";
            workSheet.Cells["D1"].Value = "Lancamento";

            row = 2;

            foreach (var movie in lessRentedMoviesInLastWeek)
            {
                workSheet.Cells[string.Format("A{0}", row)].Value = movie.Id.ToString();
                workSheet.Cells[string.Format("B{0}", row)].Value = movie.Title;
                workSheet.Cells[string.Format("C{0}", row)].Value = movie.ParentalRating.ToString();
                workSheet.Cells[string.Format("D{0}", row)].Value = movie.Launch.ToString();
                row++;
            }

            #endregion

            #region Report_moreRentsCustomer

            workSheet = package.Workbook.Worksheets.Add("Report-O_segundo_cliente_que_mais_alugou_filmes");

            workSheet.Cells["A1"].Value = "Id";
            workSheet.Cells["B1"].Value = "Nome";
            workSheet.Cells["C1"].Value = "CPF";
            workSheet.Cells["D1"].Value = "Data de Nascimento";

            if (moreRentsCustomer != null)
            {
                workSheet.Cells["A2"].Value = moreRentsCustomer.Id.ToString();
                workSheet.Cells["B2"].Value = moreRentsCustomer.Name;
                workSheet.Cells["C2"].Value = moreRentsCustomer.Cpf.Number;
                workSheet.Cells["D2"].Value = moreRentsCustomer.BirthDate.Date.ToString();
            }

            #endregion

            package.Save();

            stream.Position = 0;

            return stream;
        }
    }
}
