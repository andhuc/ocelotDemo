using Contract.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Contract.Service.Services.Implements
{
    public class ContractService : IContractService
    {

        private readonly sampleContext _dbContext;
        private readonly IDistributedCache _distributedCache;
        private readonly int cacheTime;

        public ContractService(sampleContext dbContext, IDistributedCache distributedCache, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
            cacheTime = configuration.GetValue<int>("Redis:CacheTime");
		}

		public async Task<List<Contracts>> GetContractsAsync()
		{
			var cacheKey = "contracts";
			var cachedContracts = await _distributedCache.GetStringAsync(cacheKey);

			if (cachedContracts != null)
			{
				// Deserialize cached data
				return JsonSerializer.Deserialize<List<Contracts>>(cachedContracts);
			}
			else
			{
				var contracts = await _dbContext.Contracts.OrderBy(contract => contract.Id).ToListAsync();

				var cacheOptions = new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime)
				};

				// Serialize and store in the cache
				await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(contracts), cacheOptions);

				return contracts;
			}
		}

		public async Task<Contracts> GetContractByIdAsync(int id)
        {
            return await _dbContext.Contracts.FindAsync(id);
        }

		public async Task AddContractAsync(Contracts contract)
		{
			_dbContext.Contracts.Add(contract);
			await _dbContext.SaveChangesAsync();

			// Remove the cached data when a new contract is added
			await _distributedCache.RemoveAsync("contracts");
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

				// Remove the cached data when a contract is deleted
				await _distributedCache.RemoveAsync("contracts");
			}
		}

	}
}
