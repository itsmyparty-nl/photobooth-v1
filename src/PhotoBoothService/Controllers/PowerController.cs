using com.prodg.photobooth.infrastructure.hardware;
using Microsoft.AspNetCore.Mvc;

namespace PhotoBoothService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PowerController : ControllerBase
    {
        private readonly IHardware _hardware;

        public PowerController(IHardware hardware)
        {
            _hardware = hardware;
        }
        
        // GET: api/Trigger
        [HttpGet]
        public string Get()
        {
            return _hardware.PowerControl.State.ToString();
        }
        
        // POST: api/Trigger
        [HttpPost]
        public void Post()
        {
            _hardware.PowerControl.Fire();
        }
        
    }
}
