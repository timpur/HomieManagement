using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HomieManagement.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HomieManagement.Controllers
{
    [Route("api/devicemanager")]
    public class DeviceManagerController : Controller
    {

        private HomieDeviceManager HomieDeviceManager { get; }

        public DeviceManagerController(HomieDeviceManager homieDeviceManager)
        {
            HomieDeviceManager = homieDeviceManager;
        }

        // GET: api/devicemanager/devices
        [HttpGet]
        [Route("devices")]
        public List<HomieDevice> Get()
        {
            return HomieDeviceManager.Devices;
        }
    }
}
