using com.prodg.photobooth.infrastructure.hardware;
using Microsoft.AspNetCore.Mvc;

namespace PhotoBoothService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        private readonly IHardware _hardware;

        public TriggerController(IHardware hardware)
        {
            _hardware = hardware;
        }
        
        // GET: api/Trigger
        [HttpGet]
        public string Get()
        {
            return _hardware.TriggerControl.State.ToString();
        }
        
        // POST: api/Trigger
        [HttpPost]
        public void Post()
        {
            _hardware.TriggerControl.Fire();
        }
    }
}
