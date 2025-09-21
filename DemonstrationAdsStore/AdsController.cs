using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DemonstrationAdsStore;

[ApiController]
[Route("api/[controller]")]
public class AdsController(AdsStore store) : ControllerBase
{
    [HttpPost("load")]
    public async Task<IActionResult> Load(IFormFile? file)
    {
        string content = "";

        if (file != null)
        {
            using var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            content = await sr.ReadToEndAsync();
        }

        var loaded = store.LoadFromText(content);

        return Ok(new { message = "Loaded", advertisers = loaded });
    }

    [HttpGet]
    public IActionResult GetAds([FromQuery] string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest(new { error = "Query parameter 'location' is required" });

        var list = store.GetAdvertisersFor(location);
        
        return Ok(list);
    }
}