using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCPAService.Models;
using fpsLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CCPAService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CCPAActivityController : ControllerBase
    {
        private readonly IAzureTableStorage<CCPADetails> _azureTableStorage;

        public CCPAActivityController(IAzureTableStorage<CCPADetails> azureTableStorage)
        {
            _azureTableStorage = azureTableStorage;
        }
        // POST: api/CCPAActivity

        [HttpPost]
        public async Task<List<CCPADetails>> Post([FromBody] CCPADetails details)
        {
            var ccpaRequestEntities = await _azureTableStorage.GetList(details.ActivityID);
            return ccpaRequestEntities;
        }
    }
}