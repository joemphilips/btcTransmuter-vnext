namespace BtcTransmuter.Extension.IFTTT.ExternalServices.IFTTT
{
    [Route("ifttt-plugin/ifttt/v1")]
    public class IFTTTExposeController
    {
        
        [HttpPost("triggers")]
        public async Task<IActionResult> PostFromIFTTT (IFTTTData data)
        {}

        [HttpPost("actions")]
        public async Task<IActionResult> PostFromIFTTT (IFTTTData data)
        {}
    }
}