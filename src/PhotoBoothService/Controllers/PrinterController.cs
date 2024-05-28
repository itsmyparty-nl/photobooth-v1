using com.prodg.photobooth.infrastructure.hardware;
using Microsoft.AspNetCore.Mvc;

namespace PhotoBoothService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrinterController : ControllerBase
    {
        private readonly IHardware _hardware;

        public PrinterController(IHardware hardware)
        {
            _hardware = hardware;
        }
        
        // GET: api/Trigger
        [HttpGet]
        public string Get()
        {
            return _hardware.PrintControl.State.ToString();
        }
        
        // POST: api/Trigger
        [HttpPost]
        public void Post()
        {
            _hardware.PrintControl.Fire();
        }
        
    }
}
