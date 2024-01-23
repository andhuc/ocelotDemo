using Contract.Service.Models;
using Contract.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contract.Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContractController : Controller
    {

        private readonly sampleContext _context;
        private readonly ISignService _signService;
        private readonly IContractService _contractService;
        private readonly ISignatureService _signatureService;


        public ContractController(sampleContext context, ISignService signService, IContractService contractService, ISignatureService signatureService)
        {
            _context = context;
            _signService = signService;
            _contractService = contractService;
            _signatureService = signatureService;
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
                var folderPath = Path.Combine("wwwroot", "contract");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, Guid.NewGuid().ToString() + ".pdf");
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var newContract = new Contracts
                {
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Path = filePath,
                    CreatedAt = DateTime.Now
                };

                _context.Contracts.Add(newContract);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("get")]
        public ActionResult<IEnumerable<Contracts>> GetContracts()
        {
            var contracts = _context.Contracts
                                    .OrderBy(contract => contract.Id)
                                    .ToList();

            return Ok(contracts);
        }


        [HttpGet]
        [Route("{id}")]
        public IActionResult GetContractFile(int id, bool isSigned = false)
        {
            var contract = _context.Contracts.Find(id);

            if (contract == null)
            {
                return NotFound();
            }

            var filePath = isSigned ? $"wwwroot/signed/{id}.pdf" : contract.Path;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileResult = new FileStreamResult(fileStream, "application/pdf");
            fileResult.FileDownloadName = $"{contract.Title}.pdf";

            return fileResult;
        }

        [HttpPost]
        [Route("sign/{contractId}")]
        public IActionResult AddSignatures(int contractId, [FromBody] List<SignatureDTO> signatureDTOs)
        {
            if (signatureDTOs.Count == 0)
                return BadRequest("Signature required");

            var contract = _context.Contracts.Find(contractId);

            if (contract == null)
                return NotFound();

            try
            {
                var signatures = signatureDTOs.Select(dto => new Signature
                {
                    X = dto.X,
                    Y = dto.Y,
                    Width = dto.Width,
                    Height = dto.Height,
                    Name = dto.Name,
                    Reason = dto.Reason,
                    Page = dto.Page,
                    ContractId = contractId
                }).ToList();

                _signService.SignMany($"wwwroot/signed/", signatures, contract);

                var fileStream = new FileStream($"wwwroot/signed/{contract.Id}.pdf", FileMode.Open, FileAccess.Read);

                contract.IsSigned = true;
                _context.SaveChanges();

                return File(fileStream, "application/pdf", $"{contract.Title}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("getSignature/{contractId}")]
        public async Task<ActionResult<IEnumerable<Signature>>> GetSignaturesByContractId(int contractId)
        {
            var signatures = await _signatureService.GetByContractIdAsync(contractId);

            if (signatures == null || !signatures.Any())
            {
                return NotFound();
            }

            return Ok(signatures);
        }

        [HttpPost]
        [Route("save/{contractId}")]
        public async Task<ActionResult<IEnumerable<Signature>>> SaveSignaturesByContractId(int contractId, [FromBody] List<SignatureDTO> signatureDTOs)
        {
            if (signatureDTOs == null || !signatureDTOs.Any())
            {
                return BadRequest("No signatures provided");
            }

            var deleteResult = await _signatureService.DeleteAllByContractIdAsync(contractId);

            if (!deleteResult)
            {
                return BadRequest("Failed to delete existing signatures");
            }

            var savedSignatures = new List<Signature>();

            foreach (var dto in signatureDTOs)
            {
                var signature = new Signature
                {
                    X = dto.X,
                    Y = dto.Y,
                    Width = dto.Width,
                    Height = dto.Height,
                    Name = dto.Name,
                    Reason = dto.Reason,
                    Page = dto.Page,
                    ContractId = contractId
                };

                var savedSignature = await _signatureService.CreateAsync(signature);
                savedSignatures.Add(savedSignature);
            }

            return Ok(savedSignatures);
        }

    }

    public class SignatureDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public int Page { get; set; }
    }
}
