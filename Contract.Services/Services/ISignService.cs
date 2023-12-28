using Contract.Services.Controllers;

namespace Contract.Service.Models
{
    public interface ISignService
    {
        void SignPdf(string destPath, List<Signature> signatures, Contract contract);

    }
}
