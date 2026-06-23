using Microsoft.AspNetCore.Mvc;
using TurnosAPI.DTOs;
using TurnosAPI.Services;

namespace TurnosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProfesionalesController : ControllerBase
{
    private readonly IProfesionalService _service;

    public ProfesionalesController(IProfesionalService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProfesionalResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var profesionales = await _service.GetAllAsync();
        return Ok(profesionales);
    }

    [HttpGet("{id:int}", Name = "GetProfesionalById")]
    [ProducesResponseType(typeof(ProfesionalResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var profesional = await _service.GetByIdAsync(id);
        return Ok(profesional);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfesionalResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProfesionalDto dto)
    {
        var profesional = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = profesional.Id }, profesional);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfesionalResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProfesionalDto dto)
    {
        var profesional = await _service.UpdateAsync(id, dto);
        return Ok(profesional);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
