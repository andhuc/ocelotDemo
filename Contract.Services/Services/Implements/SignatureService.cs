using Contract.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract.Service.Services.Implements
{
    public class SignatureService : ISignatureService
    {

        private readonly sampleContext _dbContext;

        public SignatureService(sampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Signature>> GetAllAsync()
        {
            return await _dbContext.Signatures.ToListAsync();
        }

        public async Task<Signature> GetByIdAsync(int id)
        {
            return await _dbContext.Signatures.FindAsync(id);
        }

        public async Task<Signature> CreateAsync(Signature entity)
        {
            _dbContext.Signatures.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Signature> UpdateAsync(int id, Signature entity)
        {
            var existingEntity = await _dbContext.Signatures.FindAsync(id);

            if (existingEntity == null)
            {
                // Handle entity not found
                return null;
            }

            // Update existingEntity properties with the values from the incoming 'entity'
            _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);

            await _dbContext.SaveChangesAsync();
            return existingEntity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.Signatures.FindAsync(id);

            if (entity == null)
            {
                // Handle entity not found
                return false;
            }

            _dbContext.Signatures.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByContractIdAsync(int contractId)
        {
            try
            {
                var signaturesToDelete = await _dbContext.Signatures
                    .Where(s => s.ContractId == contractId)
                    .ToListAsync();

                if (signaturesToDelete.Any())
                {
                    _dbContext.Signatures.RemoveRange(signaturesToDelete);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }

                // No signatures found for the given contractId
                return true;
            }
            catch (Exception)
            {
                // Handle exceptions if needed, or log the error
                return false;
            }
        }

        public async Task<IEnumerable<Signature>> GetByContractIdAsync(int contractId)
        {
            return await _dbContext.Signatures
                .Where(s => s.ContractId == contractId)
                .ToListAsync();
        }

    }
}
