using Contract.Service.Models;
using Contract.Service.Models.Implements;
using Microsoft.AspNetCore.Mvc;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Contract.Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContractController : Controller
    {

        private readonly sampleContext _context;
        private readonly ISignService _signService;

        public ContractController(sampleContext context, ISignService signService)
        {
            _context = context;
            _signService = signService;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file data");
            }

            try
            {
                // Create the folder if it doesn't exist
                var folderPath = Path.Combine("wwwroot", "contract");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Save the PDF file to the folder
                var filePath = Path.Combine(folderPath, Guid.NewGuid().ToString() + ".pdf");
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Create a new Contract entity
                var newContract = new Contract.Service.Models.Contract
                {
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Path = filePath, // Store the file path in the database
                    CreatedAt = DateTime.Now
                };


                // Add the new contract to the database
                _context.Contracts.Add(newContract);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("get")]
        public IActionResult GetContracts()
        {
            var contracts = _context.Contracts.ToList();
            return Ok(contracts);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetContractFile(int id, bool isSigned = false)
        {
            var contract = _context.Contracts.Find(id);

            if (contract == null)
            {
                return NotFound(); // Return 404 if the contract is not found
            }

            var filePath = isSigned ? $"wwwroot/signed/{id}.pdf" : contract.Path;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(); // Return 404 if the file is not found
            }

            // Return the file as a FileStreamResult
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileResult = new FileStreamResult(fileStream, "application/pdf"); // Adjust content type based on your file type
            fileResult.FileDownloadName = $"{contract.Title}.pdf"; // Set the file name

            return fileResult;
        }

        [HttpPost]
        [Route("sign/{contractId}")]
        public IActionResult AddSignatures(int contractId, [FromBody] List<Signature> signatures)
        {
            if (signatures.Count == 0)
                return BadRequest("Signature required");

            var contract = _context.Contracts.Find(contractId);

            if (contract == null)
                return NotFound();

            try
            {

                // Add signatures to the contract
                _signService.SignPdf($"wwwroot/signed/", signatures, contract);

                var fileStream = new FileStream($"wwwroot/signed/{contract.Id}.pdf", FileMode.Open, FileAccess.Read);

                contract.isSigned = true;
                _context.SaveChanges();

                return File(fileStream, "application/pdf", $"{contract.Title}.pdf");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

    public class Signature
    {
        public int Page { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Name { get; set; }
        public string Reason { get; set; }
    }
}
