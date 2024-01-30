using Contract.Service.Models;
using Contract.Service.Models.DTO;
using Contract.Service.Services;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.IO;

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

			if (file.ContentType != "application/pdf")
			{
				return BadRequest("Only PDF files are allowed");
			}

			if (file.Length > (250 * 1024 * 1024))
			{
				return BadRequest("File size exceeds the limit (250 MB)");
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

            var filePath = isSigned ? $"Storage/signed/{id}.pdf" : contract.Path;

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
				var signatures = new List<Signature>();
				foreach (var dto in signatureDTOs)
				{
					if (dto.ImageData != null)
                    {
						if (!IsValidBase64(dto.ImageData))
						{
							return BadRequest("Invalid base64 string for ImageData");
						}

						if (!IsValidSize(dto.ImageData))
						{
							return BadRequest("Image size exceeds the limit of 5MB");
						}
					}

					var signature = new Signature
					{
						X = dto.X,
						Y = dto.Y,
						Width = dto.Width,
						Height = dto.Height,
						Name = dto.Name,
						Reason = dto.Reason,
						Page = dto.Page,
						ContractId = contractId,
						ImageData = dto.ImageData,
					};

					signatures.Add(signature);
				}

				string filePath = _signService.SignMany("signed", signatures, contract);

                if (filePath == null)
                {
                    return BadRequest("Error");
                }

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
                    ContractId = contractId,
                    ImageData = dto.ImageData,
                };

                var savedSignature = await _signatureService.CreateAsync(signature);
                savedSignatures.Add(savedSignature);
            }

            return Ok(savedSignatures);
        }

		[HttpDelete]
		[Route("{id}")]
		public async Task<IActionResult> DeleteContractAsync(int id)
		{
			var contract = await _contractService.GetContractByIdAsync(id);

			if (contract == null)
			{
				return NotFound();
			}

			try
			{
				// Delete associated signatures
				await _signatureService.DeleteAllByContractIdAsync(id);

				// Delete the contract
				await _contractService.DeleteContractAsync(id);

				// Delete the file
				System.IO.File.Delete(contract.Path);

				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		[HttpPost("getBase64String")]
		[RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
		public IActionResult UploadImage()
		{
			try
			{
				var file = Request.Form.Files[0]; // Assuming only one file is uploaded

				if (file.Length > 0)
				{
					using (var ms = new MemoryStream())
					{
						file.CopyTo(ms);
						var imageBytes = ms.ToArray();
						var base64String = Convert.ToBase64String(imageBytes);

						return Ok(new { Base64String = base64String });
					}
				}

				return BadRequest("No file uploaded");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}
		}

		private bool IsValidBase64(string base64String)
		{
			try
			{
				Convert.FromBase64String(base64String);
				return true;
			}
			catch
			{
				return false;
			}
		}

        private bool IsValidSize(string base64String)
        {
			// Convert base64 to byte array
			byte[] imageBytes = Convert.FromBase64String(base64String);

            return imageBytes.Length <= 5 * 1024 * 1024;
		}

	}
}
