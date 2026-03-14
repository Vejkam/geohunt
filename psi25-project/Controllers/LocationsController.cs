using Microsoft.AspNetCore.Mvc;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        var locations = await _locationService.GetAllLocationsAsync();
        return Ok(locations);
    }

    [HttpGet("recent")]
    public async Task<ActionResult> GetRecentLocations()
    {
        var locations = await _locationService.GetRecentLocationsAsync();
        return Ok(locations);
    }

    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] LocationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var location = await _locationService.CreateLocationAsync(dto);
        return CreatedAtAction(nameof(GetLocations), new { id = location.Id }, location);
    }

    [HttpPatch("{id}/last-played")]
    public async Task<IActionResult> UpdateLastPlayed(int id)
    {
        var updated = await _locationService.UpdateLastPlayedAsync(id);
        if (!updated.success)
            return NotFound(updated.message);

        return Ok(updated.result);
    }
}
