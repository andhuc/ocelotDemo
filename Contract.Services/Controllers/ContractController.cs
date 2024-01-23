using Contract.Service.Models;
using Contract.Service.Models.DTO;
using Contract.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Contract.Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContractController : Controller
    {

        private readonly ISigningService _signService;
        private readonly IContractService _contractService;
        private readonly ISignatureService _signatureService;
        private readonly IStorageService _storageService;


        public ContractController(ISigningService signService, IContractService contractService, ISignatureService signatureService, IStorageService storageService)
        {
            _signService = signService;
            _contractService = contractService;
            _signatureService = signatureService;
            _storageService = storageService;
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

                string filePath = await _storageService.UploadFileAsync(file, "contract");

                var newContract = new Contracts
                {
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    Path = filePath,
                    CreatedAt = DateTime.Now
                };

                await _contractService.AddContractAsync(newContract);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("get")]
        public async Task<ActionResult<IEnumerable<Contracts>>> GetContracts()
        {
            var contracts = await _contractService.GetContractsAsync();

            return Ok(contracts);
        }


        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetContractFileAsync(int id, bool isSigned = false)
        {
            var contract = await _contractService.GetContractByIdAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            var filePath = isSigned ? $"Uploads/signed/{id}.pdf" : contract.Path;

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
        public async Task<IActionResult> AddSignaturesAsync(int contractId, [FromBody] List<SignatureDTO> signatureDTOs)
        {
            if (signatureDTOs.Count == 0)
                return BadRequest("Signature required");

            Contracts contract = await _contractService.GetContractByIdAsync(contractId);

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

                string filePath = _signService.SignMany("signed", signatures, contract);

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                contract.IsSigned = true;
                await _contractService.UpdateContractAsync(contract);

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
}
