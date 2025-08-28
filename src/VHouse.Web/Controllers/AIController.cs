using MediatR;
using Microsoft.AspNetCore.Mvc;
using VHouse.Application.Commands;
using VHouse.Application.Queries;
using VHouse.Domain.Enums;

namespace VHouse.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate-description")]
    public async Task<IActionResult> GenerateProductDescription([FromBody] GenerateDescriptionRequest request)
    {
        var command = new GenerateProductDescriptionCommand(
            request.ProductName,
            request.Price,
            request.Category,
            request.PreferredProvider);

        var result = await _mediator.Send(command);

        if (!result.IsSuccessful)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    [HttpPost("generate-image")]
    public async Task<IActionResult> GenerateImage([FromBody] GenerateImageRequest request)
    {
        var command = new GenerateImageCommand(
            request.Prompt,
            request.Style,
            request.PreferredProvider);

        var result = await _mediator.Send(command);

        if (!result.IsSuccessful)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealthStatus()
    {
        var query = new GetAIHealthStatusQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}

public record GenerateDescriptionRequest(
    string ProductName,
    decimal Price,
    string? Category = null,
    AIProvider? PreferredProvider = null);

public record GenerateImageRequest(
    string Prompt,
    string? Style = null,
    AIProvider? PreferredProvider = null);