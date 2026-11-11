using Microsoft.AspNetCore.Mvc;
using TuntiQ.Application.Common.Messages;
using TuntiQ.Application.Dimensions.Messages;
using TuntiQ.Application.Dimensions.Services;

namespace TuntiQ.Api.Controllers.App;

[ApiController]
[Route("api/app/dimension-values")]
public class DimensionValueController(DimensionValueService dimensionValueService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedListDto<DimensionValueDto>>> GetPagedList([FromQuery] DimensionValuePagedListRequest request) =>
        await dimensionValueService.GetPagedListAsync(request);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DimensionValueDto>> GetById(Guid id)
    {
        var value = await dimensionValueService.GetByIdAsync(id);
        return value is null ? NotFound() : value;
    }

    [HttpPost("by-ids")]
    public async Task<ActionResult<List<DimensionValueDto>>> GetByIds([FromBody] List<Guid> ids) =>
        await dimensionValueService.GetByIdsAsync(ids);

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] DimensionValueCreateRequest request)
    {
        var id = await dimensionValueService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] DimensionValueUpdateRequest request)
    {
        await dimensionValueService.UpdateAsync(request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await dimensionValueService.DeleteAsync(id);
        return NoContent();
    }
}
