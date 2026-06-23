using Microsoft.AspNetCore.Mvc;
using TurnosAPI.DTOs;
using TurnosAPI.Services;

namespace TurnosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TurnosController : ControllerBase
{
    private readonly ITurnoService _service;

    public TurnosController(ITurnoService service)
    {
        _service = service;
    }

    // 409 si hay superposición, 400 si el turno es en el pasado, 404 si no existen cliente o profesional
    [HttpPost]
    [ProducesResponseType(typeof(TurnoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTurnoDto dto)
    {
        var turno = await _service.CrearTurnoAsync(dto);
        return StatusCode(StatusCodes.Status201Created, turno);
    }

    // Usa sp_TurnosPorCliente internamente (incluye nombre del profesional vía JOIN)
    [HttpGet("cliente/{clienteId:int}")]
    [ProducesResponseType(typeof(IEnumerable<TurnoConProfesionalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCliente(int clienteId)
    {
        var turnos = await _service.GetTurnosPorClienteAsync(clienteId);
        return Ok(turnos);
    }

    // Cambia el estado a "Cancelado" sin eliminar el registro
    [HttpPut("{id:int}/cancelar")]
    [ProducesResponseType(typeof(TurnoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var turno = await _service.CancelarTurnoAsync(id);
        return Ok(turno);
    }

    // Usa sp_ProximoTurnoDisponible. Si no se pasa fechaDesde, busca desde ahora.
    [HttpGet("proximo-disponible")]
    [ProducesResponseType(typeof(ProximoDisponibleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProximoDisponible([FromQuery] ProximoDisponibleRequestDto dto)
    {
        var resultado = await _service.GetProximoTurnoDisponibleAsync(dto);
        return Ok(resultado);
    }
}
