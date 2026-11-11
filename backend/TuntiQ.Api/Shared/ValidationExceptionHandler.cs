using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using TuntiQ.Application.Common.Messages;
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

        // Always 400: the UI distinguishes error types by ErrorDto.Code, not by status code.
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
