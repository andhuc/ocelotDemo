namespace Contract.Service.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string destination);
    }
}
