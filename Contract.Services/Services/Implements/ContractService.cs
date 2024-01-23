using Contract.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract.Service.Services.Implements
{
    public class ContractService : IContractService
    {

        private readonly sampleContext _dbContext;

        public ContractService(sampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Contracts>> GetContractsAsync()
        {
            return await _dbContext.Contracts.ToListAsync();
        }

        public async Task<Contracts> GetContractByIdAsync(int id)
        {
            return await _dbContext.Contracts.FindAsync(id);
        }

        public async Task AddContractAsync(Contracts contract)
        {
            _dbContext.Contracts.Add(contract);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateContractAsync(Contracts contract)
        {
            _dbContext.Entry(contract).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteContractAsync(int id)
        {
            var contract = await _dbContext.Contracts.FindAsync(id);
            if (contract != null)
            {
                _dbContext.Contracts.Remove(contract);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
