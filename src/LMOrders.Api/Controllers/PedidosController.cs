using Asp.Versioning;
using LMOrders.Application.DTOs;
using LMOrders.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LMOrders.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoApplicationService _pedidoApplicationService;

    public PedidosController(IPedidoApplicationService pedidoApplicationService)
    {
        _pedidoApplicationService = pedidoApplicationService;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarPedido(
        [FromBody] CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _pedidoApplicationService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = response.Id },
            response);
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var response = await _pedidoApplicationService.ObterPorIdAsync(id, cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}

