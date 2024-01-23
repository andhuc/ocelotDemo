using Contract.Services.Controllers;

namespace Contract.Service.Models
{
    public interface ISignService
    {
        void SignMany(string destPath, List<Signature> signatures, Contracts contract);

    }
}
