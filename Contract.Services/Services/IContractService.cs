using Contract.Service.Models;

namespace Contract.Service.Services
{
    public interface IContractService
    {

        Task<List<Contracts>> GetContractsAsync();
        Task<Contracts> GetContractByIdAsync(int id);
        Task AddContractAsync(Contracts contract);
        Task UpdateContractAsync(Contracts contract);
        Task DeleteContractAsync(int id);
    }
}
