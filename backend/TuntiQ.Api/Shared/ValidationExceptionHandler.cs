using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using TuntiQ.Application.Common;
using TuntiQ.Domain.Common;

namespace TuntiQ.Api.Shared;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {

        if (exception is not ValidationException validation)
        {
            return false;
        }

        var response = new ErrorResponseDto(validation.Errors
            .Select(e => new ErrorDto(JsonNamingPolicy.CamelCase.ConvertName(e.Key), e.Code.ToString(), e.MessageEn, e.MessageFi))
            .ToList());

        context.Response.StatusCode = validation.Errors.Any(e => e.Code is ErrorCode.Taken or ErrorCode.Overlap)
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;

        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
