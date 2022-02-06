using System.Threading.Tasks;

namespace VideoStore.Core.Data
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}
