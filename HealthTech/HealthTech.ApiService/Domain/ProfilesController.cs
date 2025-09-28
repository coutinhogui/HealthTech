using Microsoft.AspNetCore.Mvc;
using HealthTech.ApiService.Domain.Profiles;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly Supabase.Client _supabase;

    public ProfilesController(Supabase.Client supabase) => _supabase = supabase;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _supabase.From<Profile>().Get();
        return Ok(result.Models);
    }
}
