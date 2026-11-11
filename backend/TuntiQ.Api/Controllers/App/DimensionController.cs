using Microsoft.AspNetCore.Mvc;
using TuntiQ.Application.Common.Messages;
using TuntiQ.Application.Dimensions.Messages;
using TuntiQ.Application.Dimensions.Services;

namespace TuntiQ.Api.Controllers.App;

[ApiController]
[Route("api/app/dimensions")]
public class DimensionController(DimensionService dimensionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedListDto<DimensionDto>>> GetPagedList([FromQuery] DimensionPagedListRequest request) =>
        await dimensionService.GetPagedListAsync(request);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DimensionDto>> GetById(Guid id)
    {
        var dimension = await dimensionService.GetByIdAsync(id);
        return dimension is null ? NotFound() : dimension;
    }

    [HttpPost("by-ids")]
    public async Task<ActionResult<List<DimensionDto>>> GetByIds([FromBody] List<Guid> ids) =>
        await dimensionService.GetByIdsAsync(ids);

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] DimensionCreateRequest request)
    {
        var id = await dimensionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] DimensionUpdateRequest request)
    {
        await dimensionService.UpdateAsync(request);
        return NoContent();
    }
}