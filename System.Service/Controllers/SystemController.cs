using Microsoft.AspNetCore.Mvc;
using Products.Service.Services;

namespace System.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemController : Controller
    {
        private readonly SystemHistoryService _systemHistoryService;

        public SystemController(SystemHistoryService systemHistoryService)
        {
            _systemHistoryService = systemHistoryService;
        }

        [HttpGet("SystemHistory")]
        public async Task<ActionResult<List<SystemHistory>>> GetSystemHistoryAsync()
        {
            var systemHistory = await _systemHistoryService.GetAsync();
            return Ok(systemHistory);
        }
    }
}
