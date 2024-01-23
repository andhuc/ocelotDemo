using Contract.Services.Controllers;

namespace Contract.Service.Models
{
    public interface ISigningService
    {
        string SignMany(string destPath, List<Signature> signatures, Contracts contract);

    }
}
