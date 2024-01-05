using Contract.Service.Models;

namespace Contract.Service.Services
{
    public interface IContractService
    {

        Task<IEnumerable<Signature>> GetAllAsync();
        Task<Signature> GetByIdAsync(int id);
        Task<Signature> CreateAsync(Signature entity);
        Task<Signature> UpdateAsync(int id, Signature entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllByContractIdAsync(int contractId);
        Task<IEnumerable<Signature>> GetByContractIdAsync(int contractId);
    }
}
