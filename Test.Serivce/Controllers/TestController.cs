using Microsoft.AspNetCore.Mvc;

namespace Test.Serivce.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public String Get()
        {
            return "oke";
        }

        [HttpGet("Details")]
        public String GetDetails()
        {
            return "Details data";
        }

        [HttpGet("Oke")]
        public String GetOke()
        {
            return "oke";
        }
    }
}
