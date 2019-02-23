using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.Extension.IFTTT.ExternalServices.IFTTT
{
    [Route("ifttt-plugin/external-services/ifttt")]
    [Authorize]
    public class IFTTTController : Controller
    {
        [HttpGet("{identifier}")]
        public async Task<IActionResult> EditData(string identifier)
        {
            var result = await GetExternalServiceData(identifier);
            if (result.Error != null)
            {
                return result.Error;
            }

            var client = new ExchangeService(result.Data);

            return View(new EditExchangeExternalServiceDataViewModel(client.GetData(),
                ExchangeService.GetAvailableExchanges()));
        }

    }
}