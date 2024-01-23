namespace Contract.Service.Services.Implements
{
    public class StorageService : IStorageService
    {
        private readonly string _uploadDirectory;

        public StorageService(IConfiguration configuration)
        {
            _uploadDirectory = configuration["FileUploadSettings:UploadDirectory"];
        }

        public async Task<string> UploadFileAsync(IFormFile file, string destination)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + ".pdf";

            var directoryPath = Path.Combine(_uploadDirectory, destination);
            var filePath = Path.Combine(directoryPath, uniqueFileName);

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filePath;
        }
    }
}
