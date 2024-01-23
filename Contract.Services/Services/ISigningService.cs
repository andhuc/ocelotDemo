using Contract.Services.Controllers;

namespace Contract.Service.Models
{
    public interface ISigningService
    {
        void SignMany(string destPath, List<Signature> signatures, Contracts contract);

    }
}
